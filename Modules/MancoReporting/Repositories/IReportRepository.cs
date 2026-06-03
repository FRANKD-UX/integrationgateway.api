using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Views;

namespace IntegrationGateway.Api.Modules.MancoReporting.Repositories;

public interface IReportRepository
{
    Task<List<WeeklyReportDetailView>> GetByProjectAsync(Guid projectId);
    Task<WeeklyReportDetailView?> GetDetailAsync(Guid reportId);
    Task<WeeklyReport?> GetByIdAsync(Guid reportId);
    Task<MancoUser?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId);
    Task<bool> ExistsAsync(Guid projectId, byte weekNumber, short year);
    Task<WeeklyReport> CreateAsync(WeeklyReport report);
    Task UpdateAsync(WeeklyReport report);
}
