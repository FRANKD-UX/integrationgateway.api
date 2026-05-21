using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentTypeChecklistItem
{
    public int ChecklistItemId { get; set; }

    public int IncidentTypeId { get; set; }

    public int ChainStepOrder { get; set; }

    public string ItemText { get; set; } = null!;

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual IncidentType IncidentType { get; set; } = null!;
}
