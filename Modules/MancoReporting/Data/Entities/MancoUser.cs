namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

public class MancoUser
{
    public Guid UserId { get; set; }
    public string AzureAdObjectId { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Department { get; set; }
    public string Role { get; set; } = "TeamMember";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public ICollection<MancoProject> OwnedProjects { get; set; } = new List<MancoProject>();
    public ICollection<MancoProject> CreatedProjects { get; set; } = new List<MancoProject>();
    public ICollection<MancoTask> AssignedTasks { get; set; } = new List<MancoTask>();
    public ICollection<MancoTask> CreatedTasks { get; set; } = new List<MancoTask>();
    public ICollection<WeeklyReport> SubmittedReports { get; set; } = new List<WeeklyReport>();
    public ICollection<WeeklyReport> ReviewedReports { get; set; } = new List<WeeklyReport>();
    public ICollection<MancoComment> AuthoredComments { get; set; } = new List<MancoComment>();
    public ICollection<ProjectBacklogReason> AddedBacklogReasons { get; set; } = new List<ProjectBacklogReason>();
    public ICollection<PriorityDecision> PriorityDecisions { get; set; } = new List<PriorityDecision>();
}
