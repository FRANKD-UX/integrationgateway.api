using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record CreateReportRequest(
    Guid ProjectId,
    [property: Range(1, 53)] byte WeekNumber,
    [property: Range(2020, 2100)] short Year,
    [property: Required] string Summary,
    string? Achievements,
    string? Blockers,
    string? PlannedNextWeek);
