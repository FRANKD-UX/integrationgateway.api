namespace IntegrationGateway.Api.Modules.Auth.Models;

public sealed class ClientApplicationDto
{
    public int? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string AppType { get; set; } = string.Empty;

    public string AuthProvider { get; set; } = "EntraId";

    public bool IsActive { get; set; } = true;
}
