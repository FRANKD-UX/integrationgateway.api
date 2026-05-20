using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public bool CanAssign { get; set; }

    public bool CanApprove { get; set; }

    public bool CanEscalate { get; set; }

    public bool CanClose { get; set; }

    public bool CanEditAll { get; set; }

    public bool IsDepartmentLead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
