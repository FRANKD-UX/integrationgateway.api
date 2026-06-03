using IntegrationGateway.Api.Modules.MancoReporting.Data;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Views;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.MancoReporting.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly MancoDbContext _context;

    public ReportRepository(MancoDbContext context)
    {
        _context = context;
    }

    public Task<List<WeeklyReportDetailView>> GetByProjectAsync(Guid projectId) =>
        _context.WeeklyReportDetails.AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.WeekNumber)
            .ToListAsync();

    public Task<WeeklyReportDetailView?> GetDetailAsync(Guid reportId) =>
        _context.WeeklyReportDetails.AsNoTracking().FirstOrDefaultAsync(r => r.ReportId == reportId);

    public Task<WeeklyReport?> GetByIdAsync(Guid reportId) =>
        _context.WeeklyReports.FirstOrDefaultAsync(r => r.ReportId == reportId);

    public Task<MancoUser?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId) =>
        _context.Users.FirstOrDefaultAsync(u => u.AzureAdObjectId == azureAdObjectId && u.IsActive);

    public Task<bool> ExistsAsync(Guid projectId, byte weekNumber, short year) =>
        _context.WeeklyReports.AnyAsync(r => r.ProjectId == projectId && r.WeekNumber == weekNumber && r.Year == year);

    public async Task<WeeklyReport> CreateAsync(WeeklyReport report)
    {
        _context.WeeklyReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task UpdateAsync(WeeklyReport report)
    {
        _context.WeeklyReports.Update(report);
        await _context.SaveChangesAsync();
    }
}
