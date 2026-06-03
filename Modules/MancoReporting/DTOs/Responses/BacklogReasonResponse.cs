namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record BacklogReasonResponse(
    Guid ReasonId,
    Guid ProjectId,
    string ReasonText,
    Guid AddedBy,
    string AddedByName,
    DateTime CreatedAt);
