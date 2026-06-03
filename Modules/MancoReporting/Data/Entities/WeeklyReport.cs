namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

public class WeeklyReport
{
    public Guid ReportId { get; set; }
    public Guid ProjectId { get; set; }
    public byte WeekNumber { get; set; }
    public short Year { get; set; }
    public string Summary { get; set; } = null!;
    public string? Achievements { get; set; }
    public string? Blockers { get; set; }
    public string? PlannedNextWeek { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public MancoProject Project { get; set; } = null!;
    public MancoUser Submitter { get; set; } = null!;
    public MancoUser? Reviewer { get; set; }
    public ICollection<MancoComment> Comments { get; set; } = new List<MancoComment>();
    public ICollection<PriorityDecision> PriorityDecisions { get; set; } = new List<PriorityDecision>();
}
