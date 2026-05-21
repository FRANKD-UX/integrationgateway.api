using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentTypeChainStep
{
    public int ChainStepId { get; set; }

    public int IncidentTypeId { get; set; }

    public int StepOrder { get; set; }

    public int DepartmentId { get; set; }

    public int SlaMinutes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual IncidentType IncidentType { get; set; } = null!;
}
