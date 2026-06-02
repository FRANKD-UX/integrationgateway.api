using System.Security.Claims;
using IntegrationGateway.Api.Modules.Auth.Models;

namespace IntegrationGateway.Api.Modules.Auth;

public sealed class PermissionService
{
    private static readonly string[] AdminRoles = ["Gateway.Admin", "IncidentOps.Admin"];

    public IReadOnlyList<PermissionDto> GetPermissions(ClaimsPrincipal user)
    {
        var permissions = new Dictionary<string, PermissionDto>(StringComparer.OrdinalIgnoreCase);
        var roles = Values(user, "roles", ClaimTypes.Role);
        var groups = Values(user, "groups");
        var scopes = user.Claims
            .Where(claim => claim.Type == "scp" || claim.Type == "scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roles.Overlaps(AdminRoles))
        {
            Add(permissions, "admin.full", "role");
            Add(permissions, "incidents.read", "role");
            Add(permissions, "incidents.write", "role");
            Add(permissions, "attachments.write", "role");
            Add(permissions, "workflow.transition", "role");
            Add(permissions, "checklist.update", "role");
            Add(permissions, "accounts.process", "role");
            Add(permissions, "reports.read", "role");
        }

        AddScopePermission(permissions, scopes, "incidents.read");
        AddScopePermission(permissions, scopes, "incidents.write");
        AddScopePermission(permissions, scopes, "attachments.write");
        AddScopePermission(permissions, scopes, "workflow.transition");
        AddScopePermission(permissions, scopes, "admin.full");

        ApplyTeamMappings(permissions, roles, "role");
        ApplyTeamMappings(permissions, groups, "group");

        return permissions.Values
            .OrderBy(permission => permission.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void AddScopePermission(
        IDictionary<string, PermissionDto> permissions,
        ISet<string> scopes,
        string permission)
    {
        if (scopes.Contains(permission))
            Add(permissions, permission, "scope");
    }

    private static void ApplyTeamMappings(
        IDictionary<string, PermissionDto> permissions,
        IEnumerable<string> values,
        string source)
    {
        foreach (var value in values)
        {
            if (ContainsToken(value, "Support"))
            {
                Add(permissions, "incidents.read", source);
                Add(permissions, "checklist.update", source);
            }

            if (ContainsToken(value, "Operations"))
            {
                Add(permissions, "incidents.read", source);
                Add(permissions, "workflow.transition", source);
            }

            if (ContainsToken(value, "Accounts"))
            {
                Add(permissions, "incidents.read", source);
                Add(permissions, "accounts.process", source);
            }

            if (ContainsToken(value, "Management"))
            {
                Add(permissions, "incidents.read", source);
                Add(permissions, "reports.read", source);
            }

            if (ContainsToken(value, "Admin"))
                Add(permissions, "admin.full", source);
        }
    }

    private static HashSet<string> Values(ClaimsPrincipal user, params string[] claimTypes)
    {
        var types = claimTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return user.Claims
            .Where(claim => types.Contains(claim.Type))
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool ContainsToken(string value, string token)
    {
        return value.Contains(token, StringComparison.OrdinalIgnoreCase);
    }

    private static void Add(IDictionary<string, PermissionDto> permissions, string name, string source)
    {
        permissions.TryAdd(name, new PermissionDto
        {
            Name = name,
            Source = source
        });
    }
}
