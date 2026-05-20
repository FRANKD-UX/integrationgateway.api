using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentLog
{
    public int IncidentLogId { get; set; }

    public int WorkItemId { get; set; }

    public string Action { get; set; } = null!;

    public int PerformedByUserId { get; set; }

    public DateTime Timestamp { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Comment { get; set; }

    public string? FieldName { get; set; }

    public string? Severity { get; set; }

    public string? IncidentType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User PerformedByUser { get; set; } = null!;

    public virtual WorkItem WorkItem { get; set; } = null!;
}
