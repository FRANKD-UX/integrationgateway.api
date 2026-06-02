using System.Security.Claims;

namespace IntegrationGateway.Api.Modules.Auth;

public static class AuthPolicyClaims
{
    public static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return user.Claims.Any(claim =>
            (claim.Type == "roles" || claim.Type == ClaimTypes.Role) &&
            roleSet.Contains(claim.Value));
    }

    public static bool HasAnyScope(ClaimsPrincipal user, params string[] scopes)
    {
        var scopeSet = scopes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return user.Claims
            .Where(claim => claim.Type == "scp" || claim.Type == "scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Any(scopeSet.Contains);
    }

    public static bool HasAnyPermission(ClaimsPrincipal user, params string[] permissions)
    {
        var permissionSet = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return user.Claims
            .Where(claim => claim.Type == "permissions" || claim.Type == "permission")
            .Select(claim => claim.Value)
            .Any(permissionSet.Contains);
    }
}
