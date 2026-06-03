namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record PriorityDecisionResponse(
    Guid DecisionId,
    Guid ProjectId,
    string PriorityLevel,
    string? Justification,
    Guid DecidedBy,
    string DecidedByName,
    DateTime DecidedAt,
    Guid? ReportId);
