using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class WorkItem
{
    public int WorkItemId { get; set; }

    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Description { get; set; }

    public string RequestType { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int? SiteId { get; set; }

    public string Type { get; set; } = null!;

    public int? IncidentTypeId { get; set; }

    public string? Severity { get; set; }

    public string? AffectedService { get; set; }

    public string? Impact { get; set; }

    public int? SlaresponseMinutes { get; set; }

    public int? SlaresolutionMinutes { get; set; }

    public DateTime? Sladeadline { get; set; }

    public string? Slastatus { get; set; }

    public int? EscalationLevel { get; set; }

    public int? EscalatedToUserId { get; set; }

    public DateTime? LastEscalatedAt { get; set; }

    public DateTime? ResponseDueDate { get; set; }

    public DateTime? ResolutionDueDate { get; set; }

    public string? ResponseToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }

    public string? RespondedBy { get; set; }

    public DateTime? RespondedAt { get; set; }

    public int? AssignedToUserId { get; set; }

    public int? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual User? AssignedToUser { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual User? EscalatedToUser { get; set; }

    public virtual ICollection<IncidentLog> IncidentLogs { get; set; } = new List<IncidentLog>();

    public virtual IncidentType? IncidentType { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Site? Site { get; set; }

    public virtual WorkItemAssignment? WorkItemAssignment { get; set; }

    public virtual ICollection<WorkItemCollaborator> WorkItemCollaborators { get; set; } = new List<WorkItemCollaborator>();
}
