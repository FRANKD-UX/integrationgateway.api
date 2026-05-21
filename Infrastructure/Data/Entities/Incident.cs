using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class Incident
{
    public int IncidentId { get; set; }

    public int ProjectId { get; set; }

    public int IncidentTypeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public string? Priority { get; set; }

    public int OriginDepartmentId { get; set; }

    public int CurrentDepartmentId { get; set; }

    public int CurrentChainStepOrder { get; set; }

    public int? CreatedByUserId { get; set; }

    public int? ClosedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int TotalChainSteps { get; set; }

    public virtual Department CurrentDepartment { get; set; } = null!;

    public virtual ICollection<IncidentAttachment> IncidentAttachments { get; set; } = new List<IncidentAttachment>();

    public virtual ICollection<IncidentAuditLog> IncidentAuditLogs { get; set; } = new List<IncidentAuditLog>();

    public virtual ICollection<IncidentChainChecklist> IncidentChainChecklists { get; set; } = new List<IncidentChainChecklist>();

    public virtual ICollection<IncidentChainStep> IncidentChainSteps { get; set; } = new List<IncidentChainStep>();

    public virtual ICollection<IncidentEscalation> IncidentEscalations { get; set; } = new List<IncidentEscalation>();

    public virtual IncidentType IncidentType { get; set; } = null!;

    public virtual Department OriginDepartment { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
