using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentChainStep
{
    public int StepId { get; set; }

    public int IncidentId { get; set; }

    public int StepOrder { get; set; }

    public int DepartmentId { get; set; }

    public string Status { get; set; } = null!;

    public int SlaMinutes { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? AssignedToUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Incident Incident { get; set; } = null!;

    public virtual ICollection<IncidentAttachment> IncidentAttachments { get; set; } = new List<IncidentAttachment>();

    public virtual ICollection<IncidentChainChecklist> IncidentChainChecklists { get; set; } = new List<IncidentChainChecklist>();

    public virtual ICollection<IncidentEscalation> IncidentEscalations { get; set; } = new List<IncidentEscalation>();
}
