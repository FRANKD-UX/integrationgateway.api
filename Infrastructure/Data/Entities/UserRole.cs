using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class UserRole
{
    public int UserRoleId { get; set; }

    public int UserId { get; set; }

    public int ProjectId { get; set; }

    public int RoleId { get; set; }

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; }

    public bool CanAssign { get; set; }

    public bool CanApprove { get; set; }

    public bool CanEscalate { get; set; }

    public bool IsDepartmentLead { get; set; }

    public int? MaxAssignableLoad { get; set; }

    public bool CanClose { get; set; }

    public bool CanEditAll { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Department? Department { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
