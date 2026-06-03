using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

namespace IntegrationGateway.Api.Modules.MancoReporting.Repositories;

public interface ICommentRepository
{
    Task<List<MancoComment>> GetByReportAsync(Guid reportId);
    Task<List<MancoComment>> GetByProjectAsync(Guid projectId);
    Task<MancoComment> CreateAsync(MancoComment comment);
    Task<MancoComment?> GetByIdAsync(Guid commentId);
    Task<MancoUser?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId);
    Task UpdateAsync(MancoComment comment);
}
