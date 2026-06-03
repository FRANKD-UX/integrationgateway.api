namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record ProjectSummaryResponse(
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    string CurrentPriority,
    DateOnly? StartDate,
    DateOnly? TargetDate,
    Guid OwnerId,
    string OwnerName,
    string OwnerEmail,
    string? LatestReportStatus,
    int OpenTaskCount,
    int TotalTaskCount,
    int UnresolvedCommentCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
