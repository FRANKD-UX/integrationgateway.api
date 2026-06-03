namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record TaskResponse(
    Guid TaskId,
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateOnly? DueDate,
    Guid? AssignedTo,
    string? AssignedToName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt);
