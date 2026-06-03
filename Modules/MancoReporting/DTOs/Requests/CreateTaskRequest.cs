using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record CreateTaskRequest
{
    public Guid ProjectId { get; init; }

    [Required]
    [MaxLength(256)]
    public string Title { get; init; } = null!;

    public string? Description { get; init; }

    public string Priority { get; init; } = "Medium";

    public DateOnly? DueDate { get; init; }

    public Guid? AssignedTo { get; init; }
}
