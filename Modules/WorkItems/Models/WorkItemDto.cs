namespace IntegrationGateway.Api.Modules.WorkItems.Models;

public class WorkItemDto
{
    public int WorkItemId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? AffectedService { get; set; }
    public string? Impact { get; set; }
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? SiteId { get; set; }
    public string? SiteName { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? SladeadLine { get; set; }
    public string? Slastatus { get; set; }
    public int? SlaresponseMinutes { get; set; }
    public int? SlaresolutionMinutes { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    public int? EscalationLevel { get; set; }
    public DateTime? LastEscalatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}