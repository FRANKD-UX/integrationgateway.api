using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record UpdateProjectStatusRequest
{
    [Required]
    public string Status { get; init; } = null!;
}
