using System.Security.Claims;
using IntegrationGateway.Api.Modules.Auth.Models;

namespace IntegrationGateway.Api.Modules.Auth;

public sealed class CurrentUserService
{
    private readonly PermissionService _permissionService;
    private readonly ClientApplicationService _clientApplicationService;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(
        PermissionService permissionService,
        ClientApplicationService clientApplicationService,
        ILogger<CurrentUserService> logger)
    {
        _permissionService = permissionService;
        _clientApplicationService = clientApplicationService;
        _logger = logger;
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        var userId = FirstClaim(user, "oid", "sub", ClaimTypes.NameIdentifier);
        var tenantId = FirstClaim(user, "tid");
        var sourceApp = await _clientApplicationService.ResolveSourceAppAsync(user);
        var permissions = _permissionService.GetPermissions(user)
            .Select(permission => permission.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (string.IsNullOrWhiteSpace(userId))
            _logger.LogWarning("Authenticated principal is missing an object identifier claim.");

        if (string.IsNullOrWhiteSpace(tenantId))
            _logger.LogWarning("Authenticated principal {UserObjectId} is missing tenant id claim.", userId);

        _logger.LogInformation(
            "Resolved current user {UserObjectId} in tenant {TenantId} from client {ClientId}",
            userId,
            tenantId,
            sourceApp);

        return new CurrentUserDto
        {
            UserId = userId,
            DisplayName = FirstClaim(user, "name", ClaimTypes.Name),
            Email = FirstClaim(user, "preferred_username", "upn", "email", ClaimTypes.Email),
            TenantId = tenantId,
            AuthProvider = "EntraId",
            SourceApp = sourceApp,
            Roles = ClaimValues(user, "roles", ClaimTypes.Role),
            Groups = ClaimValues(user, "groups"),
            Scopes = ScopeValues(user),
            Permissions = permissions
        };
    }

    private static string FirstClaim(ClaimsPrincipal user, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return string.Empty;
    }

    private static IReadOnlyList<string> ClaimValues(ClaimsPrincipal user, params string[] claimTypes)
    {
        var types = claimTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return user.Claims
            .Where(claim => types.Contains(claim.Type))
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> ScopeValues(ClaimsPrincipal user)
    {
        return user.Claims
            .Where(claim => claim.Type == "scp" || claim.Type == "scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
