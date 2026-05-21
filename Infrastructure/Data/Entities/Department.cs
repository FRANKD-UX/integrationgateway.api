using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class Department
{
    public int DepartmentId { get; set; }

    public int ProjectId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<IncidentChainStep> IncidentChainSteps { get; set; } = new List<IncidentChainStep>();

    public virtual ICollection<Incident> IncidentCurrentDepartments { get; set; } = new List<Incident>();

    public virtual ICollection<Incident> IncidentOriginDepartments { get; set; } = new List<Incident>();

    public virtual ICollection<IncidentTypeChainStep> IncidentTypeChainSteps { get; set; } = new List<IncidentTypeChainStep>();

    public virtual ICollection<IncidentType> IncidentTypes { get; set; } = new List<IncidentType>();

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<SlaPolicy> SlaPolicies { get; set; } = new List<SlaPolicy>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
