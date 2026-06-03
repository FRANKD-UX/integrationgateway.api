using IntegrationGateway.Api.Modules.MancoReporting.Data;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Views;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.MancoReporting.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly MancoDbContext _context;

    public ProjectRepository(MancoDbContext context)
    {
        _context = context;
    }

    public Task<List<ActiveProjectSummaryView>> GetSummaryAsync() =>
        _context.ActiveProjectSummary.AsNoTracking().ToListAsync();

    public Task<MancoProject?> GetByIdAsync(Guid id) =>
        _context.Projects.Include(p => p.Owner).FirstOrDefaultAsync(p => p.ProjectId == id);

    public async Task<MancoProject> CreateAsync(MancoProject project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task UpdateAsync(MancoProject project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public Task<List<MancoTask>> GetTasksAsync(Guid projectId) =>
        _context.Tasks.Include(t => t.AssignedUser)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public Task<MancoTask?> GetTaskByIdAsync(Guid taskId) =>
        _context.Tasks.Include(t => t.AssignedUser).FirstOrDefaultAsync(t => t.TaskId == taskId);

    public async Task<MancoTask> CreateTaskAsync(MancoTask task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateTaskAsync(MancoTask task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public Task<List<ProjectBacklogReason>> GetBacklogReasonsAsync(Guid projectId) =>
        _context.ProjectBacklogReasons.Include(r => r.AddedByUser)
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<ProjectBacklogReason> AddBacklogReasonAsync(ProjectBacklogReason reason)
    {
        _context.ProjectBacklogReasons.Add(reason);
        await _context.SaveChangesAsync();
        return reason;
    }

    public Task<List<PriorityDecision>> GetPriorityHistoryAsync(Guid projectId) =>
        _context.PriorityDecisions.Include(d => d.DecidedByUser)
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.DecidedAt)
            .ToListAsync();

    public async Task<PriorityDecision> AddPriorityDecisionAndUpdateProjectAsync(
        PriorityDecision decision,
        MancoProject project)
    {
        _context.PriorityDecisions.Add(decision);
        _context.Entry(project).Property(p => p.CurrentPriority).IsModified = true;
        _context.Entry(project).Property(p => p.UpdatedAt).IsModified = true;
        await _context.SaveChangesAsync();
        return decision;
    }
}
