using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class Project
{
    public int ProjectId { get; set; }

    public string ProjectCode { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public string? SharePointSiteUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<IncidentType> IncidentTypes { get; set; } = new List<IncidentType>();

    public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();

    public virtual ICollection<Site> Sites { get; set; } = new List<Site>();

    public virtual ICollection<SlaPolicy> SlaPolicies { get; set; } = new List<SlaPolicy>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
