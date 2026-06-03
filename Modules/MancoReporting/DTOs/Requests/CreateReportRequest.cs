using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record CreateReportRequest
{
    public Guid ProjectId { get; init; }

    [Range(1, 53)]
    public byte WeekNumber { get; init; }

    [Range(2020, 2100)]
    public short Year { get; init; }

    [Required]
    public string Summary { get; init; } = null!;

    public string? Achievements { get; init; }

    public string? Blockers { get; init; }

    public string? PlannedNextWeek { get; init; }
}
