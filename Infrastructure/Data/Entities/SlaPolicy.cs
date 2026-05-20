using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class SlaPolicy
{
    public int SlaPolicyId { get; set; }

    public int ProjectId { get; set; }

    public int DepartmentId { get; set; }

    public string Severity { get; set; } = null!;

    public int ResponseMinutes { get; set; }

    public int ResolutionMinutes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
