using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentChainChecklist
{
    public int ChecklistId { get; set; }

    public int StepId { get; set; }

    public int IncidentId { get; set; }

    public int ChecklistItemId { get; set; }

    public string ItemText { get; set; } = null!;

    public bool IsRequired { get; set; }

    public bool IsCompleted { get; set; }

    public int? CompletedByUserId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Incident Incident { get; set; } = null!;

    public virtual IncidentChainStep Step { get; set; } = null!;
}
