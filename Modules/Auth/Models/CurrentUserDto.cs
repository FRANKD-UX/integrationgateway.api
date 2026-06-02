namespace IntegrationGateway.Api.Modules.Auth.Models;

public sealed class CurrentUserDto
{
    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string AuthProvider { get; set; } = "EntraId";

    public string SourceApp { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; set; } = [];

    public IReadOnlyList<string> Groups { get; set; } = [];

    public IReadOnlyList<string> Scopes { get; set; } = [];

    public IReadOnlyList<string> Permissions { get; set; } = [];
}
