using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public interface IProjectService
{
    Task<List<ProjectSummaryResponse>> GetSummaryAsync();
    Task<ProjectResponse> GetByIdAsync(Guid id);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, string callerAzureAdObjectId);
    Task UpdateStatusAsync(Guid projectId, UpdateProjectStatusRequest request, string callerAzureAdObjectId);
    Task<List<TaskResponse>> GetTasksAsync(Guid projectId);
    Task<TaskResponse> CreateTaskAsync(Guid projectId, CreateTaskRequest request, string callerAzureAdObjectId);
    Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request);
    Task<List<BacklogReasonResponse>> GetBacklogReasonsAsync(Guid projectId);
    Task<BacklogReasonResponse> AddBacklogReasonAsync(Guid projectId, AddBacklogReasonRequest request, string callerAzureAdObjectId);
    Task<List<PriorityDecisionResponse>> GetPriorityHistoryAsync(Guid projectId);
    Task<PriorityDecisionResponse> SetPriorityAsync(Guid projectId, SetPriorityRequest request, string callerAzureAdObjectId);
}
