using IntegrationGateway.Api.Infrastructure.Data;
using IntegrationGateway.Api.Middleware;
using IntegrationGateway.Api.Modules.MancoReporting.Data;
using IntegrationGateway.Api.Modules.MancoReporting.Repositories;
using IntegrationGateway.Api.Modules.MancoReporting.Services;
using IntegrationGateway.Api.Modules.Attachments;
using IntegrationGateway.Api.Modules.Auth;
using IntegrationGateway.Api.Modules.Dashboard;
using IntegrationGateway.Api.Modules.IncidentWorkflow;
using IntegrationGateway.Api.Modules.WorkItems;
using IntegrationGateway.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);
var developmentAuthEnabled = builder.Configuration.GetValue<bool>("ControlPlane:DevelopmentAuthEnabled");

if (developmentAuthEnabled && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("ControlPlane:DevelopmentAuthEnabled can only be enabled in Development.");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var graphTenantId = builder.Configuration["Graph:TenantId"];
var graphClientId = builder.Configuration["Graph:ClientId"];
var azureAdTenantId = builder.Configuration["AzureAd:TenantId"];
var azureAdClientId = builder.Configuration["AzureAd:ClientId"];
var azureAdAudience = builder.Configuration["AzureAd:Audience"];
var azureAdInstance = builder.Configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";

if (string.IsNullOrWhiteSpace(graphTenantId))
    throw new InvalidOperationException("Graph:TenantId is not configured.");

if (string.IsNullOrWhiteSpace(graphClientId))
    throw new InvalidOperationException("Graph:ClientId is not configured.");

if (string.IsNullOrWhiteSpace(azureAdTenantId))
    azureAdTenantId = graphTenantId;

if (string.IsNullOrWhiteSpace(azureAdClientId))
    azureAdClientId = graphClientId;

var validAudiences = new[]
{
    azureAdClientId,
    $"api://{azureAdClientId}",
    azureAdAudience
}
.Where(audience => !string.IsNullOrWhiteSpace(audience))
.Distinct(StringComparer.OrdinalIgnoreCase)
.ToArray();

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = developmentAuthEnabled
        ? "GatewayAuthentication"
        : JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = developmentAuthEnabled
        ? "GatewayAuthentication"
        : JwtBearerDefaults.AuthenticationScheme;
});

authBuilder.AddMicrosoftIdentityWebApi(
    jwtOptions =>
    {
        jwtOptions.Authority = $"{azureAdInstance.TrimEnd('/')}/{azureAdTenantId}/v2.0";
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers =
            [
                $"{azureAdInstance.TrimEnd('/')}/{azureAdTenantId}/v2.0",
                $"https://sts.windows.net/{azureAdTenantId}/"
            ],
            ValidateAudience = true,
            ValidAudiences = validAudiences
        };
        jwtOptions.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("IntegrationGateway.Auth");

                logger.LogInformation(
                    "Auth provider {AuthProvider} validated token for user {UserObjectId} in tenant {TenantId} from client {ClientId}",
                    "EntraId",
                    context.Principal?.FindFirst("oid")?.Value ?? context.Principal?.FindFirst("sub")?.Value,
                    context.Principal?.FindFirst("tid")?.Value,
                    context.Principal?.FindFirst("azp")?.Value ?? context.Principal?.FindFirst("appid")?.Value);

                return Task.CompletedTask;
            }
        };
    },
    identityOptions =>
    {
        identityOptions.Instance = azureAdInstance;
        identityOptions.TenantId = azureAdTenantId;
        identityOptions.ClientId = azureAdClientId;
    });

if (developmentAuthEnabled)
{
    authBuilder.AddPolicyScheme("GatewayAuthentication", "Gateway authentication", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var hasBearerToken = context.Request.Headers.Authorization.Any(value =>
                value?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true);

            return hasBearerToken
                ? JwtBearerDefaults.AuthenticationScheme
                : DevelopmentAuthenticationHandler.SchemeName;
        };
    });

    authBuilder.AddScheme<DevelopmentAuthenticationOptions, DevelopmentAuthenticationHandler>(
        DevelopmentAuthenticationHandler.SchemeName,
        _ => { });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GatewayAdmin", policy =>
        policy.RequireAssertion(context => AuthPolicyClaims.HasAnyRole(context.User, "Gateway.Admin", "IncidentOps.Admin")));

    options.AddPolicy("IncidentRead", policy =>
        policy.RequireAssertion(context =>
            AuthPolicyClaims.HasAnyScope(context.User, "incidents.read") ||
            AuthPolicyClaims.HasAnyRole(context.User, "Gateway.Admin", "IncidentOps.Admin")));

    options.AddPolicy("IncidentWrite", policy =>
        policy.RequireAssertion(context =>
            AuthPolicyClaims.HasAnyScope(context.User, "incidents.write") ||
            AuthPolicyClaims.HasAnyRole(context.User, "Gateway.Admin", "IncidentOps.Admin")));

    options.AddPolicy("AttachmentWrite", policy =>
        policy.RequireAssertion(context =>
            AuthPolicyClaims.HasAnyScope(context.User, "attachments.write") ||
            AuthPolicyClaims.HasAnyRole(context.User, "Gateway.Admin", "IncidentOps.Admin")));

    options.AddPolicy("WorkflowTransition", policy =>
        policy.RequireAssertion(context =>
            AuthPolicyClaims.HasAnyScope(context.User, "workflow.transition") ||
            AuthPolicyClaims.HasAnyPermission(context.User, "workflow.transition") ||
            AuthPolicyClaims.HasAnyRole(context.User, "Gateway.Admin", "IncidentOps.Admin")));
});

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    options.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
    options.MapType<DateOnly?>(() => new OpenApiSchema { Type = "string", Format = "date", Nullable = true });
});

var allowedOrigins = builder.Configuration
    .GetSection("App:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
        else
            policy.AllowAnyOrigin();

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddScoped<GraphAuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<ClientApplicationService>();
builder.Services.AddHttpClient<AttachmentsService>()
    .AddPolicyHandler(GetRetryPolicy());

builder.Services.AddScoped<WorkItemRepository>();
builder.Services.AddScoped<WorkItemService>();

builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<IncidentWorkflowRepository>();
builder.Services.AddScoped<WorkflowEngine>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IntegrationGateway.Api.Modules.IncidentWorkflow.IncidentWorkflowService>();

builder.Services.AddHttpClient<SharePointService>()
    .AddPolicyHandler(GetRetryPolicy());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

builder.Services.AddDbContext<MancoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MancoReportingConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IMancoUserResolver, MancoUserResolver>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (error != null)
        {
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = error.Error.Message,
                    inner = error.Error.InnerException?.Message,
                    type = error.Error.GetType().Name
                }));
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("FrontendCors");
app.UseAuthentication();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                // Retry details are intentionally not logged here to avoid leaking integration payloads.
            });
}
