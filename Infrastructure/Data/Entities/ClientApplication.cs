namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class ClientApplication
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string AppType { get; set; } = null!;

    public string AuthProvider { get; set; } = null!;

    public string? AllowedOrigins { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
