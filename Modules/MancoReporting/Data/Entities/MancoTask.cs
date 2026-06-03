namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

public class MancoTask
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo";
    public string Priority { get; set; } = "Medium";
    public DateOnly? DueDate { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public MancoProject Project { get; set; } = null!;
    public MancoUser? AssignedUser { get; set; }
    public MancoUser CreatedByUser { get; set; } = null!;
}
