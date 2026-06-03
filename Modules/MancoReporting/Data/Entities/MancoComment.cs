namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

public class MancoComment
{
    public Guid CommentId { get; set; }
    public Guid? ReportId { get; set; }
    public Guid? ProjectId { get; set; }
    public string CommentText { get; set; } = null!;
    public string CommentType { get; set; } = "General";
    public Guid AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsResolved { get; set; } = false;
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public WeeklyReport? Report { get; set; }
    public MancoProject? Project { get; set; }
    public MancoUser Author { get; set; } = null!;
    public MancoUser? Resolver { get; set; }
}
