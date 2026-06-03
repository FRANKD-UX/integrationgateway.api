using IntegrationGateway.Api.Modules.MancoReporting.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.MancoReporting.Auth;

public sealed class MancoRoleAuthorizationHandler : AuthorizationHandler<MancoRoleRequirement>
{
    private readonly MancoDbContext _context;

    public MancoRoleAuthorizationHandler(MancoDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MancoRoleRequirement requirement)
    {
        string azureAdObjectId;

        try
        {
            azureAdObjectId = context.User.GetAzureAdObjectId();
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        var userRole = await _context.Users
            .Where(user => user.AzureAdObjectId == azureAdObjectId && user.IsActive)
            .Select(user => user.Role)
            .FirstOrDefaultAsync();

        if (userRole is null)
        {
            return;
        }

        if (requirement.AllowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
    }
}
