using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class Site
{
    public int SiteId { get; set; }

    public int ProjectId { get; set; }

    public string SiteName { get; set; } = null!;

    public string? SiteCode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
