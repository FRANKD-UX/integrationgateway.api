using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentAuditLog
{
    public int AuditId { get; set; }

    public int IncidentId { get; set; }

    public int? StepId { get; set; }

    public string Action { get; set; } = null!;

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }

    public int? PerformedByUserId { get; set; }

    public DateTime PerformedAt { get; set; }

    public string? Notes { get; set; }

    public virtual Incident Incident { get; set; } = null!;
}
