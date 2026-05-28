using IntegrationGateway.Api.Infrastructure.Data;
using IntegrationGateway.Api.Modules.Dashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.Dashboard;

public class DashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetIncidentTypeCountAsync()
    {
        return await _context.IncidentTypes
            .Where(t => t.IsActive)
            .CountAsync();
    }

    public async Task<int> GetSlaRuleCountAsync()
    {
        return await _context.SlaPolicies
            .Where(p => p.IsActive)
            .CountAsync();
    }

    public async Task<int> GetRoleCountAsync()
    {
        return await _context.Roles.CountAsync();
    }

    public async Task<List<DepartmentWorkloadDto>> GetDepartmentWorkloadAsync()
    {
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .ToListAsync();

        var result = new List<DepartmentWorkloadDto>();

        foreach (var dept in departments)
        {
            var openCount = await _context.Incidents
                .Where(i => !i.IsDeleted &&
                    i.CurrentDepartmentId == dept.DepartmentId &&
                    i.Status == "Logged")
                .CountAsync();

            var inProgressCount = await _context.Incidents
                .Where(i => !i.IsDeleted &&
                    i.CurrentDepartmentId == dept.DepartmentId &&
                    i.Status == "InProgress")
                .CountAsync();

            var escalatedCount = await _context.Incidents
                .Where(i => !i.IsDeleted &&
                    i.CurrentDepartmentId == dept.DepartmentId &&
                    i.Status == "Escalated")
                .CountAsync();

            result.Add(new DepartmentWorkloadDto
            {
                Department = new DepartmentDto
                {
                    Id = dept.DepartmentId,
                    Name = dept.DepartmentName
                },
                OpenIncidents = openCount,
                InProgress = inProgressCount,
                Escalated = escalatedCount,
                AvgResolutionTime = 0,
                SlaCompliance = 0
            });
        }

        return result;
    }

    public async Task<List<RecentIncidentDto>> GetRecentIncidentsAsync(int count = 10)
    {
        return await _context.Incidents
            .Include(i => i.CurrentDepartment)
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .Take(count)
            .Select(i => new RecentIncidentDto
            {
                IncidentId = i.IncidentId,
                Title = i.Title,
                Status = i.Status,
                Priority = i.Priority,
                CurrentDepartmentName = i.CurrentDepartment.DepartmentName,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<TrendDataDto>> GetDailyTrendsAsync(int days = 7)
    {
        var result = new List<TrendDataDto>();
        var now = DateTime.UtcNow.Date;

        for (int i = days - 1; i >= 0; i--)
        {
            var date = now.AddDays(-i);
            var nextDate = date.AddDays(1);

            var created = await _context.Incidents
                .Where(inc => !inc.IsDeleted &&
                    inc.CreatedAt >= date &&
                    inc.CreatedAt < nextDate)
                .CountAsync();

            var resolved = await _context.Incidents
                .Where(inc => !inc.IsDeleted &&
                    inc.Status == "Closed" &&
                    inc.ClosedAt >= date &&
                    inc.ClosedAt < nextDate)
                .CountAsync();

            result.Add(new TrendDataDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Created = created,
                Resolved = resolved,
                Escalated = 0
            });
        }

        return result;
    }

    public async Task<int> GetTotalOpenAsync()
    {
        return await _context.Incidents
            .Where(i => !i.IsDeleted && i.Status != "Closed")
            .CountAsync();
    }

    public async Task<int> GetTotalByStatusAsync(string status)
    {
        return await _context.Incidents
            .Where(i => !i.IsDeleted && i.Status == status)
            .CountAsync();
    }

    public async Task<int> GetTotalByPriorityAsync(string priority)
    {
        return await _context.Incidents
            .Where(i => !i.IsDeleted &&
                i.Priority == priority &&
                i.Status != "Closed")
            .CountAsync();
    }
}