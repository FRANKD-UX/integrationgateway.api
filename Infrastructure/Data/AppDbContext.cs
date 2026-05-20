using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using IntegrationGateway.Api.Infrastructure.Data.Entities;

namespace IntegrationGateway.Api.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<IncidentLog> IncidentLogs { get; set; }

    public virtual DbSet<IncidentType> IncidentTypes { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectUser> ProjectUsers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<SlaPolicy> SlaPolicies { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<WorkItem> WorkItems { get; set; }

    public virtual DbSet<WorkItemAssignment> WorkItemAssignments { get; set; }

    public virtual DbSet<WorkItemCollaborator> WorkItemCollaborators { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.DepartmentName }, "UX_Departments_Project_Department").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Project).WithMany(p => p.Departments)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Departments_Projects");
        });

        modelBuilder.Entity<IncidentLog>(entity =>
        {
            entity.HasIndex(e => new { e.WorkItemId, e.Timestamp }, "IX_IncidentLogs_WorkItemId_Timestamp").IsDescending(false, true);

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FieldName).HasMaxLength(200);
            entity.Property(e => e.IncidentType).HasMaxLength(200);
            entity.Property(e => e.Severity).HasMaxLength(10);
            entity.Property(e => e.Timestamp)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.PerformedByUser).WithMany(p => p.IncidentLogs)
                .HasForeignKey(d => d.PerformedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentLogs_PerformedByUser");

            entity.HasOne(d => d.WorkItem).WithMany(p => p.IncidentLogs)
                .HasForeignKey(d => d.WorkItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentLogs_WorkItems");
        });

        modelBuilder.Entity<IncidentType>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.DepartmentId, e.Title }, "UX_IncidentTypes_Project_Department_Title").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Severity).HasMaxLength(10);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Department).WithMany(p => p.IncidentTypes)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentTypes_Departments");

            entity.HasOne(d => d.Project).WithMany(p => p.IncidentTypes)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentTypes_Projects");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(e => e.ProjectCode, "UX_Projects_ProjectCode").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProjectCode).HasMaxLength(100);
            entity.Property(e => e.ProjectName).HasMaxLength(200);
            entity.Property(e => e.SharePointSiteUrl).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<ProjectUser>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.UserId }, "UX_ProjectUsers_Project_User").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectUsers)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectUsers_Projects");

            entity.HasOne(d => d.User).WithMany(p => p.ProjectUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectUsers_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.RoleName, "UX_Roles_RoleName").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RoleName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.SiteName }, "UX_Sites_Project_SiteName").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SiteCode).HasMaxLength(100);
            entity.Property(e => e.SiteName).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Project).WithMany(p => p.Sites)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sites_Projects");
        });

        modelBuilder.Entity<SlaPolicy>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.DepartmentId, e.Severity }, "UX_SlaPolicies_Project_Department_Severity").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Severity).HasMaxLength(10);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Department).WithMany(p => p.SlaPolicies)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SlaPolicies_Departments");

            entity.HasOne(d => d.Project).WithMany(p => p.SlaPolicies)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SlaPolicies_Projects");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.AdObjectId, "UX_Users_AdObjectId")
                .IsUnique()
                .HasFilter("([AdObjectId] IS NOT NULL)");

            entity.HasIndex(e => e.Email, "UX_Users_Email").IsUnique();

            entity.Property(e => e.AdObjectId).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LoginName).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ProjectId }, "IX_UserRoles_User_Project");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Department).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_UserRoles_Departments");

            entity.HasOne(d => d.Project).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Projects");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Users");
        });

        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasIndex(e => e.AssignedToUserId, "IX_WorkItems_AssignedToUserId");

            entity.HasIndex(e => e.IncidentTypeId, "IX_WorkItems_IncidentTypeId");

            entity.HasIndex(e => new { e.ProjectId, e.CreatedAt }, "IX_WorkItems_Project_CreatedAt").IsDescending(false, true);

            entity.HasIndex(e => new { e.ProjectId, e.DepartmentId, e.Status }, "IX_WorkItems_Project_Department_Status");

            entity.Property(e => e.AffectedService).HasMaxLength(250);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DueDate).HasPrecision(0);
            entity.Property(e => e.LastEscalatedAt).HasPrecision(0);
            entity.Property(e => e.Priority).HasMaxLength(20);
            entity.Property(e => e.RequestType).HasMaxLength(20);
            entity.Property(e => e.ResolutionDueDate).HasPrecision(0);
            entity.Property(e => e.RespondedAt).HasPrecision(0);
            entity.Property(e => e.RespondedBy).HasMaxLength(200);
            entity.Property(e => e.ResponseDueDate).HasPrecision(0);
            entity.Property(e => e.ResponseToken).HasMaxLength(200);
            entity.Property(e => e.Severity).HasMaxLength(10);
            entity.Property(e => e.Sladeadline)
                .HasPrecision(0)
                .HasColumnName("SLADeadline");
            entity.Property(e => e.SlaresolutionMinutes).HasColumnName("SLAResolutionMinutes");
            entity.Property(e => e.SlaresponseMinutes).HasColumnName("SLAResponseMinutes");
            entity.Property(e => e.Slastatus)
                .HasMaxLength(20)
                .HasColumnName("SLAStatus");
            entity.Property(e => e.StartDate).HasPrecision(0);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.TokenExpiresAt).HasPrecision(0);
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.WorkItemAssignedToUsers)
                .HasForeignKey(d => d.AssignedToUserId)
                .HasConstraintName("FK_WorkItems_AssignedToUser");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.WorkItemCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_WorkItems_CreatedByUser");

            entity.HasOne(d => d.Department).WithMany(p => p.WorkItems)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkItems_Departments");

            entity.HasOne(d => d.EscalatedToUser).WithMany(p => p.WorkItemEscalatedToUsers)
                .HasForeignKey(d => d.EscalatedToUserId)
                .HasConstraintName("FK_WorkItems_EscalatedToUser");

            entity.HasOne(d => d.IncidentType).WithMany(p => p.WorkItems)
                .HasForeignKey(d => d.IncidentTypeId)
                .HasConstraintName("FK_WorkItems_IncidentTypes");

            entity.HasOne(d => d.Project).WithMany(p => p.WorkItems)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkItems_Projects");

            entity.HasOne(d => d.Site).WithMany(p => p.WorkItems)
                .HasForeignKey(d => d.SiteId)
                .HasConstraintName("FK_WorkItems_Sites");
        });

        modelBuilder.Entity<WorkItemAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId);

            entity.HasIndex(e => new { e.WorkItemId, e.AssignedAt }, "IX_WorkItemAssignments_WorkItemId").IsDescending(false, true);

            entity.HasIndex(e => e.WorkItemId, "UX_WorkItemAssignments_Current")
                .IsUnique()
                .HasFilter("([IsCurrent]=(1))");

            entity.Property(e => e.AssignedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.AssignmentType).HasMaxLength(30);
            entity.Property(e => e.IsCurrent).HasDefaultValue(true);
            entity.Property(e => e.UnassignedAt).HasPrecision(0);

            entity.HasOne(d => d.AssignedByUser).WithMany(p => p.WorkItemAssignmentAssignedByUsers)
                .HasForeignKey(d => d.AssignedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkItemAssignments_AssignedByUser");

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.WorkItemAssignmentAssignedToUsers)
                .HasForeignKey(d => d.AssignedToUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkItemAssignments_AssignedToUser");

            entity.HasOne(d => d.WorkItem).WithOne(p => p.WorkItemAssignment)
                .HasForeignKey<WorkItemAssignment>(d => d.WorkItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkItemAssignments_WorkItems");
        });

        modelBuilder.Entity<WorkItemCollaborator>(entity =>
        {
            entity.HasKey(e => e.CollaboratorId);

            entity.HasIndex(e => new { e.WorkItemId, e.CollaboratorUserId }, "UX_WorkItemCollaborators_WorkItem_User").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RequestedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RespondedAt).HasPrecision(0);
            entity.Property(e => e.ResponseToken).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CollaboratorUser).WithMany(p => p.WorkItemCollaboratorCollaboratorUsers)
                .HasForeignKey(d => d.CollaboratorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkItemCollaborators_CollaboratorUser");

            entity.HasOne(d => d.RequestedByUser).WithMany(p => p.WorkItemCollaboratorRequestedByUsers)
                .HasForeignKey(d => d.RequestedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkItemCollaborators_RequestedByUser");

            entity.HasOne(d => d.WorkItem).WithMany(p => p.WorkItemCollaborators)
                .HasForeignKey(d => d.WorkItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkItemCollaborators_WorkItems");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
