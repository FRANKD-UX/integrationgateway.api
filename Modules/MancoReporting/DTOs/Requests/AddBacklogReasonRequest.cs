using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record AddBacklogReasonRequest
{
    [Required]
    [MaxLength(1024)]
    public string ReasonText { get; init; } = null!;
}
