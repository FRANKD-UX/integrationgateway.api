namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Views;

public class ActiveProjectSummaryView
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public string CurrentPriority { get; set; } = null!;
    public DateOnly? StartDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public string OwnerEmail { get; set; } = null!;
    public string? LatestReportStatus { get; set; }
    public int OpenTaskCount { get; set; }
    public int TotalTaskCount { get; set; }
    public int UnresolvedCommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
