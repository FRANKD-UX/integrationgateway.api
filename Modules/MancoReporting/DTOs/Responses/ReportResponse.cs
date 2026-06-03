namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record ReportResponse(
    Guid ReportId,
    Guid ProjectId,
    string ProjectTitle,
    string ProjectStatus,
    string CurrentPriority,
    byte WeekNumber,
    short Year,
    string Summary,
    string? Achievements,
    string? Blockers,
    string? PlannedNextWeek,
    string ReportStatus,
    Guid SubmittedBy,
    string SubmittedByName,
    string SubmittedByEmail,
    DateTime? SubmittedAt,
    Guid? ReviewedBy,
    string? ReviewedByName,
    DateTime? ReviewedAt);
