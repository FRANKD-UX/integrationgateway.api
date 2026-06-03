namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Views;

public class WeeklyReportDetailView
{
    public Guid ReportId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = null!;
    public string ProjectStatus { get; set; } = null!;
    public string CurrentPriority { get; set; } = null!;
    public byte WeekNumber { get; set; }
    public short Year { get; set; }
    public string Summary { get; set; } = null!;
    public string? Achievements { get; set; }
    public string? Blockers { get; set; }
    public string? PlannedNextWeek { get; set; }
    public string ReportStatus { get; set; } = null!;
    public Guid SubmittedBy { get; set; }
    public string SubmittedByName { get; set; } = null!;
    public string SubmittedByEmail { get; set; } = null!;
    public DateTime? SubmittedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
