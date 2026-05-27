using IntegrationGateway.Api.Infrastructure.Data;
using IntegrationGateway.Api.Middleware;
using IntegrationGateway.Api.Modules.WorkItems;
using IntegrationGateway.Api.Services;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using IntegrationGateway.Api.Modules.IncidentWorkflow;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger / OpenAPI
builder.Services.AddSwaggerGen();

// CORS — origins are configured in appsettings under App:AllowedOrigins
var allowedOrigins = builder.Configuration
    .GetSection("App:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
        else
            policy.AllowAnyOrigin();

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

// Register services
builder.Services.AddScoped<GraphAuthService>();

// WorkItems module
builder.Services.AddScoped<WorkItemRepository>();
builder.Services.AddScoped<WorkItemService>();

// IncidentWorkflow module
builder.Services.AddScoped<IncidentWorkflowRepository>();
builder.Services.AddScoped<WorkflowEngine>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IntegrationGateway.Api.Modules.IncidentWorkflow.IncidentWorkflowService>();


// HttpClient for SharePointService with Polly retry policy
builder.Services.AddHttpClient<SharePointService>()
    .AddPolicyHandler(GetRetryPolicy());

// Database - SQL Server via EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    )
);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Global exception handler — must come before other middleware
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
                })
            );
        }
    });
});

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Angular");

// API key middleware - must be before UseAuthorization and MapControllers
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
                // Log the retry attempt
            });
}

