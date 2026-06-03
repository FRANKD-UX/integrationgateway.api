namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

public class PriorityDecision
{
    public Guid DecisionId { get; set; }
    public Guid ProjectId { get; set; }
    public string PriorityLevel { get; set; } = null!;
    public string? Justification { get; set; }
    public Guid DecidedBy { get; set; }
    public DateTime DecidedAt { get; set; }
    public Guid? ReportId { get; set; }
    public MancoProject Project { get; set; } = null!;
    public MancoUser DecidedByUser { get; set; } = null!;
    public WeeklyReport? Report { get; set; }
}
