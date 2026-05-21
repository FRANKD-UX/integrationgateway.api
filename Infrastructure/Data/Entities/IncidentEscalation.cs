using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentEscalation
{
    public int EscalationId { get; set; }

    public int IncidentId { get; set; }

    public int StepId { get; set; }

    public DateTime EscalatedAt { get; set; }

    public int? EscalatedToUserId { get; set; }

    public int NotificationCount { get; set; }

    public string? Reason { get; set; }

    public virtual Incident Incident { get; set; } = null!;

    public virtual IncidentChainStep Step { get; set; } = null!;
}
