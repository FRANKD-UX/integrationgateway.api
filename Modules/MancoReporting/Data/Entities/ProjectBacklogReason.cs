namespace IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

public class ProjectBacklogReason
{
    public Guid ReasonId { get; set; }
    public Guid ProjectId { get; set; }
    public string ReasonText { get; set; } = null!;
    public Guid AddedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public MancoProject Project { get; set; } = null!;
    public MancoUser AddedByUser { get; set; } = null!;
}
