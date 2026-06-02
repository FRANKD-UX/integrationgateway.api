using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace IntegrationGateway.Api.Modules.Auth;

public sealed class DevelopmentAuthenticationOptions : AuthenticationSchemeOptions;

public sealed class DevelopmentAuthenticationHandler : AuthenticationHandler<DevelopmentAuthenticationOptions>
{
    public const string SchemeName = "Development";

    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<DevelopmentAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.Authorization.Any(value =>
                value?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim("oid", "dev-user"),
            new Claim("sub", "dev-user"),
            new Claim("name", "Development Admin"),
            new Claim("preferred_username", "dev@local"),
            new Claim("email", "dev@local"),
            new Claim("tid", "development-tenant"),
            new Claim("azp", "development-client"),
            new Claim("roles", "Gateway.Admin"),
            new Claim("roles", "IncidentOps.Admin")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        Logger.LogInformation("Auth provider {AuthProvider} created development principal {UserObjectId}.", SchemeName, "dev-user");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
