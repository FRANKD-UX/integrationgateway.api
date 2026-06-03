using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Views;

namespace IntegrationGateway.Api.Modules.MancoReporting.Repositories;

public interface IProjectRepository
{
    Task<List<ActiveProjectSummaryView>> GetSummaryAsync();
    Task<MancoProject?> GetByIdAsync(Guid id);
    Task<MancoProject> CreateAsync(MancoProject project);
    Task UpdateAsync(MancoProject project);
    Task<List<MancoTask>> GetTasksAsync(Guid projectId);
    Task<MancoTask?> GetTaskByIdAsync(Guid taskId);
    Task<MancoTask> CreateTaskAsync(MancoTask task);
    Task UpdateTaskAsync(MancoTask task);
    Task<List<ProjectBacklogReason>> GetBacklogReasonsAsync(Guid projectId);
    Task<ProjectBacklogReason> AddBacklogReasonAsync(ProjectBacklogReason reason);
    Task<List<PriorityDecision>> GetPriorityHistoryAsync(Guid projectId);
    Task<PriorityDecision> AddPriorityDecisionAndUpdateProjectAsync(PriorityDecision decision, MancoProject project);
}
