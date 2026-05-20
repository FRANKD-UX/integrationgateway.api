using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentType
{
    public int IncidentTypeId { get; set; }

    public int ProjectId { get; set; }

    public int DepartmentId { get; set; }

    public string Title { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
