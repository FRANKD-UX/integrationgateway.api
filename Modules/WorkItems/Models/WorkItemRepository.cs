using IntegrationGateway.Api.Infrastructure.Data;
using IntegrationGateway.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.WorkItems;

public class WorkItemRepository
{
    private readonly AppDbContext _context;

    public WorkItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkItem>> GetAllAsync(
        int? projectId = null,
        string? status = null,
        string? requestType = null,
        int? departmentId = null)
    {
        var query = _context.WorkItems
            .Include(w => w.Department)
            .Include(w => w.Site)
            .Include(w => w.AssignedToUser)
            .Include(w => w.CreatedByUser)
            .Where(w => !w.IsDeleted)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(w => w.ProjectId == projectId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.Status == status);

        if (!string.IsNullOrWhiteSpace(requestType))
            query = query.Where(w => w.RequestType == requestType);

        if (departmentId.HasValue)
            query = query.Where(w => w.DepartmentId == departmentId.Value);

        return await query
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<WorkItem?> GetByIdAsync(int workItemId)
    {
        return await _context.WorkItems
            .Include(w => w.Department)
            .Include(w => w.Site)
            .Include(w => w.AssignedToUser)
            .Include(w => w.CreatedByUser)
            .Include(w => w.IncidentLogs)
            .Include(w => w.WorkItemCollaborators)
            .FirstOrDefaultAsync(w => w.WorkItemId == workItemId && !w.IsDeleted);
    }

    public async Task<WorkItem> CreateAsync(WorkItem workItem)
    {
        _context.WorkItems.Add(workItem);
        await _context.SaveChangesAsync();
        return workItem;
    }

    public async Task<WorkItem?> UpdateAsync(int workItemId, Action<WorkItem> applyUpdates)
    {
        var workItem = await _context.WorkItems
            .FirstOrDefaultAsync(w => w.WorkItemId == workItemId && !w.IsDeleted);

        if (workItem is null)
            return null;

        applyUpdates(workItem);
        workItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return workItem;
    }

    public async Task<bool> DeleteAsync(int workItemId)
    {
        var workItem = await _context.WorkItems
            .FirstOrDefaultAsync(w => w.WorkItemId == workItemId && !w.IsDeleted);

        if (workItem is null)
            return false;

        // Soft delete - never remove records from the database
        workItem.IsDeleted = true;
        workItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}