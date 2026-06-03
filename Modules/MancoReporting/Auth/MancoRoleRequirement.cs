using Microsoft.AspNetCore.Authorization;

namespace IntegrationGateway.Api.Modules.MancoReporting.Auth;

public sealed class MancoRoleRequirement : IAuthorizationRequirement
{
    public MancoRoleRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }

    public IReadOnlyCollection<string> AllowedRoles { get; }
}
