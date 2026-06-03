using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Views;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.MancoReporting.Data;

public class MancoDbContext : DbContext
{
    public MancoDbContext(DbContextOptions<MancoDbContext> options)
        : base(options)
    {
    }

    public DbSet<MancoUser> Users => Set<MancoUser>();
    public DbSet<MancoProject> Projects => Set<MancoProject>();
    public DbSet<MancoTask> Tasks => Set<MancoTask>();
    public DbSet<WeeklyReport> WeeklyReports => Set<WeeklyReport>();
    public DbSet<MancoComment> MancoComments => Set<MancoComment>();
    public DbSet<ProjectBacklogReason> ProjectBacklogReasons => Set<ProjectBacklogReason>();
    public DbSet<PriorityDecision> PriorityDecisions => Set<PriorityDecision>();
    public DbSet<ActiveProjectSummaryView> ActiveProjectSummary => Set<ActiveProjectSummaryView>();
    public DbSet<WeeklyReportDetailView> WeeklyReportDetails => Set<WeeklyReportDetailView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MancoUser>(entity =>
        {
            entity.ToTable("Users", "dbo");
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.AzureAdObjectId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.UserId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.AzureAdObjectId).HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("TeamMember");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.LastLoginAt).HasPrecision(7);
        });

        modelBuilder.Entity<MancoProject>(entity =>
        {
            entity.ToTable("Projects", "dbo");
            entity.HasKey(e => e.ProjectId);
            entity.Property(e => e.ProjectId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Draft");
            entity.Property(e => e.CurrentPriority).HasMaxLength(20).HasDefaultValue("Unset");
            entity.Property(e => e.CreatedAt).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Owner).WithMany(e => e.OwnedProjects)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Projects_Owner");
            entity.HasOne(e => e.Creator).WithMany(e => e.CreatedProjects)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Projects_Creator");
        });

        modelBuilder.Entity<MancoTask>(entity =>
        {
            entity.ToTable("Tasks", "dbo");
            entity.HasKey(e => e.TaskId);
            entity.Property(e => e.TaskId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Todo");
            entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("Medium");
            entity.Property(e => e.CreatedAt).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.CompletedAt).HasPrecision(7);
            entity.HasOne(e => e.Project).WithMany(e => e.Tasks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssignedUser).WithMany(e => e.AssignedTasks)
                .HasForeignKey(e => e.AssignedTo)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.CreatedByUser).WithMany(e => e.CreatedTasks)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<WeeklyReport>(entity =>
        {
            entity.ToTable("WeeklyReports", "dbo");
            entity.HasKey(e => e.ReportId);
            entity.HasIndex(e => new { e.ProjectId, e.WeekNumber, e.Year }).IsUnique();
            entity.Property(e => e.ReportId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Draft");
            entity.Property(e => e.SubmittedAt).HasPrecision(7);
            entity.Property(e => e.ReviewedAt).HasPrecision(7);
            entity.HasOne(e => e.Project).WithMany(e => e.Reports)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Submitter).WithMany(e => e.SubmittedReports)
                .HasForeignKey(e => e.SubmittedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Reviewer).WithMany(e => e.ReviewedReports)
                .HasForeignKey(e => e.ReviewedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MancoComment>(entity =>
        {
            entity.ToTable("MancoComments", "dbo");
            entity.HasKey(e => e.CommentId);
            entity.Property(e => e.CommentId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.CommentType).HasMaxLength(20).HasDefaultValue("General");
            entity.Property(e => e.CreatedAt).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.IsResolved).HasDefaultValue(false);
            entity.Property(e => e.ResolvedAt).HasPrecision(7);
            entity.HasOne(e => e.Report).WithMany(e => e.Comments)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.Project).WithMany(e => e.Comments)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.Author).WithMany(e => e.AuthoredComments)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.Resolver).WithMany()
                .HasForeignKey(e => e.ResolvedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ProjectBacklogReason>(entity =>
        {
            entity.ToTable("ProjectBacklogReasons", "dbo");
            entity.HasKey(e => e.ReasonId);
            entity.Property(e => e.ReasonId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.ReasonText).HasMaxLength(1024);
            entity.Property(e => e.CreatedAt).HasPrecision(7).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Project).WithMany(e => e.BacklogReasons)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AddedByUser).WithMany(e => e.AddedBacklogReasons)
                .HasForeignKey(e => e.AddedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PriorityDecision>(entity =>
        {
            entity.ToTable("PriorityDecisions", "dbo");
            entity.HasKey(e => e.DecisionId);
            entity.Property(e => e.DecisionId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.PriorityLevel).HasMaxLength(20);
            entity.Property(e => e.DecidedAt).HasPrecision(7);
            entity.HasOne(e => e.Project).WithMany(e => e.PriorityDecisions)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.DecidedByUser).WithMany(e => e.PriorityDecisions)
                .HasForeignKey(e => e.DecidedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.Report).WithMany(e => e.PriorityDecisions)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ActiveProjectSummaryView>(entity =>
        {
            entity.HasNoKey().ToView("vw_ActiveProjectSummary", "dbo");
            entity.Property(e => e.Title).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.CurrentPriority).HasMaxLength(20);
            entity.Property(e => e.OwnerName).HasMaxLength(200);
            entity.Property(e => e.OwnerEmail).HasMaxLength(320);
            entity.Property(e => e.LatestReportStatus).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasPrecision(7);
            entity.Property(e => e.UpdatedAt).HasPrecision(7);
        });

        modelBuilder.Entity<WeeklyReportDetailView>(entity =>
        {
            entity.HasNoKey().ToView("vw_WeeklyReportDetail", "dbo");
            entity.Property(e => e.ProjectTitle).HasMaxLength(256);
            entity.Property(e => e.ProjectStatus).HasMaxLength(20);
            entity.Property(e => e.CurrentPriority).HasMaxLength(20);
            entity.Property(e => e.ReportStatus).HasMaxLength(20);
            entity.Property(e => e.SubmittedByName).HasMaxLength(200);
            entity.Property(e => e.SubmittedByEmail).HasMaxLength(320);
            entity.Property(e => e.ReviewedByName).HasMaxLength(200);
            entity.Property(e => e.SubmittedAt).HasPrecision(7);
            entity.Property(e => e.ReviewedAt).HasPrecision(7);
        });
    }
}
