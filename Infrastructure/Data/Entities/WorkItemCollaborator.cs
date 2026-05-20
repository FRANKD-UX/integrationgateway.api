using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class WorkItemCollaborator
{
    public int CollaboratorId { get; set; }

    public int WorkItemId { get; set; }

    public int RequestedByUserId { get; set; }

    public int CollaboratorUserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RequestedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public string? ResponseToken { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User CollaboratorUser { get; set; } = null!;

    public virtual User RequestedByUser { get; set; } = null!;

    public virtual WorkItem WorkItem { get; set; } = null!;
}
