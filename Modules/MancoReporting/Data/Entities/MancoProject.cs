namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

public class MancoProject
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = "Draft";
    public string CurrentPriority { get; set; } = "Unset";
    public DateOnly? StartDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public Guid OwnerId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public MancoUser Owner { get; set; } = null!;
    public MancoUser Creator { get; set; } = null!;
    public ICollection<MancoTask> Tasks { get; set; } = new List<MancoTask>();
    public ICollection<WeeklyReport> Reports { get; set; } = new List<WeeklyReport>();
    public ICollection<ProjectBacklogReason> BacklogReasons { get; set; } = new List<ProjectBacklogReason>();
    public ICollection<MancoComment> Comments { get; set; } = new List<MancoComment>();
    public ICollection<PriorityDecision> PriorityDecisions { get; set; } = new List<PriorityDecision>();
}
