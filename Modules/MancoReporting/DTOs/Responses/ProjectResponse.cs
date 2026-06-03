namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record ProjectResponse(
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    string CurrentPriority,
    DateOnly? StartDate,
    DateOnly? TargetDate,
    Guid OwnerId,
    string OwnerName,
    DateTime CreatedAt,
    DateTime UpdatedAt);
