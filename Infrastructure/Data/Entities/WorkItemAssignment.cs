using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class WorkItemAssignment
{
    public int AssignmentId { get; set; }

    public int WorkItemId { get; set; }

    public int AssignedByUserId { get; set; }

    public int AssignedToUserId { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? UnassignedAt { get; set; }

    public string AssignmentType { get; set; } = null!;

    public bool IsCurrent { get; set; }

    public virtual User AssignedByUser { get; set; } = null!;

    public virtual User AssignedToUser { get; set; } = null!;

    public virtual WorkItem WorkItem { get; set; } = null!;
}
