using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record CreateProjectRequest
{
    [Required]
    [MaxLength(256)]
    public string Title { get; init; } = null!;

    public string? Description { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? TargetDate { get; init; }

    public Guid OwnerId { get; init; }
}
