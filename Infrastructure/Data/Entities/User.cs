using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? LoginName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? AdObjectId { get; set; }

    public virtual ICollection<IncidentLog> IncidentLogs { get; set; } = new List<IncidentLog>();

    public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<WorkItem> WorkItemAssignedToUsers { get; set; } = new List<WorkItem>();

    public virtual ICollection<WorkItemAssignment> WorkItemAssignmentAssignedByUsers { get; set; } = new List<WorkItemAssignment>();

    public virtual ICollection<WorkItemAssignment> WorkItemAssignmentAssignedToUsers { get; set; } = new List<WorkItemAssignment>();

    public virtual ICollection<WorkItemCollaborator> WorkItemCollaboratorCollaboratorUsers { get; set; } = new List<WorkItemCollaborator>();

    public virtual ICollection<WorkItemCollaborator> WorkItemCollaboratorRequestedByUsers { get; set; } = new List<WorkItemCollaborator>();

    public virtual ICollection<WorkItem> WorkItemCreatedByUsers { get; set; } = new List<WorkItem>();

    public virtual ICollection<WorkItem> WorkItemEscalatedToUsers { get; set; } = new List<WorkItem>();
}
