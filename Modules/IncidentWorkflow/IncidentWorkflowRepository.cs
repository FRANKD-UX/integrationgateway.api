using IntegrationGateway.Api.Infrastructure.Data;
using IntegrationGateway.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.IncidentWorkflow;

public class IncidentWorkflowRepository
{
    private readonly AppDbContext _context;

    public IncidentWorkflowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Incident?> GetIncidentWithChainAsync(int incidentId)
    {
        return await _context.Incidents
            .Include(i => i.IncidentType)
            .Include(i => i.OriginDepartment)
            .Include(i => i.CurrentDepartment)
            .Include(i => i.IncidentChainSteps)
                .ThenInclude(s => s.Department)
            .Include(i => i.IncidentChainSteps)
                .ThenInclude(s => s.IncidentChainChecklists)
            .FirstOrDefaultAsync(i => i.IncidentId == incidentId && !i.IsDeleted);
    }

    public async Task<List<Incident>> GetByDepartmentAsync(
        int departmentId,
        string? status = null)
    {
        var query = _context.Incidents
            .Include(i => i.IncidentType)
            .Include(i => i.OriginDepartment)
            .Include(i => i.CurrentDepartment)
            .Where(i => !i.IsDeleted)
            .Where(i =>
                i.CurrentDepartmentId == departmentId ||
                i.OriginDepartmentId == departmentId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<IncidentTypeChainStep>> GetChainTemplateAsync(
        int incidentTypeId)
    {
        return await _context.IncidentTypeChainSteps
            .Where(s => s.IncidentTypeId == incidentTypeId)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();
    }

    public async Task<List<IncidentTypeChecklistItem>> GetChecklistTemplateAsync(
        int incidentTypeId,
        int stepOrder)
    {
        return await _context.IncidentTypeChecklistItems
            .Where(i =>
                i.IncidentTypeId == incidentTypeId &&
                i.ChainStepOrder == stepOrder)
            .OrderBy(i => i.SortOrder)
            .ToListAsync();
    }

    public async Task<Incident> CreateIncidentAsync(Incident incident)
    {
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();
        return incident;
    }

    public async Task<IncidentChainStep> CreateChainStepAsync(
        IncidentChainStep step)
    {
        _context.IncidentChainSteps.Add(step);
        await _context.SaveChangesAsync();
        return step;
    }

    public async Task AddChecklistItemsAsync(
        List<IncidentChainChecklist> items)
    {
        _context.IncidentChainChecklists.AddRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task<IncidentChainChecklist?> GetChecklistItemAsync(
        int checklistId)
    {
        return await _context.IncidentChainChecklists
            .Include(c => c.Step)
            .FirstOrDefaultAsync(c => c.ChecklistId == checklistId);
    }

    public async Task SaveAuditLogAsync(IncidentAuditLog entry)
    {
        _context.IncidentAuditLogs.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<IncidentAuditLog>> GetAuditLogAsync(
        int incidentId)
    {
        return await _context.IncidentAuditLogs
            .Where(a => a.IncidentId == incidentId)
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync();
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<List<User>> GetDepartmentUsersAsync(int departmentId)
    {
        return await _context.UserRoles
            .Include(ur => ur.User)
            .Where(ur =>
                ur.DepartmentId == departmentId &&
                ur.IsActive &&
                ur.User.IsActive)
            .Select(ur => ur.User)
            .ToListAsync();
    }
}