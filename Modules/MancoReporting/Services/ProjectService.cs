using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Views;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;
using IntegrationGateway.Api.Modules.MancoReporting.Enums;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using IntegrationGateway.Api.Modules.MancoReporting.Repositories;
using MancoValidationException = IntegrationGateway.Api.Modules.MancoReporting.Exceptions.ValidationException;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IMancoUserResolver _userResolver;

    public ProjectService(IProjectRepository repository, IMancoUserResolver userResolver)
    {
        _repository = repository;
        _userResolver = userResolver;
    }

    public async Task<List<ProjectSummaryResponse>> GetSummaryAsync() =>
        (await _repository.GetSummaryAsync()).Select(MapSummary).ToList();

    public async Task<ProjectResponse> GetByIdAsync(Guid id)
    {
        var project = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Project {id} not found");
        return MapProject(project);
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, string callerAzureAdObjectId)
    {
        var caller = await _userResolver.ResolveActiveUserAsync(callerAzureAdObjectId);
        var project = new MancoProject
        {
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            TargetDate = request.TargetDate,
            OwnerId = request.OwnerId,
            CreatedBy = caller.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(project);
        return await GetByIdAsync(project.ProjectId);
    }

    public async Task UpdateStatusAsync(Guid projectId, UpdateProjectStatusRequest request, string callerAzureAdObjectId)
    {
        _ = await _userResolver.ResolveActiveUserAsync(callerAzureAdObjectId);
        if (!Enum.TryParse<ProjectStatus>(request.Status, false, out _))
            throw new MancoValidationException($"Invalid project status '{request.Status}'");

        var project = await _repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException($"Project {projectId} not found");

        project.Status = request.Status;
        project.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(project);
    }

    public async Task<List<TaskResponse>> GetTasksAsync(Guid projectId) =>
        (await _repository.GetTasksAsync(projectId)).Select(MapTask).ToList();

    public async Task<TaskResponse> CreateTaskAsync(Guid projectId, CreateTaskRequest request, string callerAzureAdObjectId)
    {
        var caller = await _userResolver.ResolveActiveUserAsync(callerAzureAdObjectId);
        var task = new MancoTask
        {
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedTo = request.AssignedTo,
            CreatedBy = caller.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return MapTask(await _repository.CreateTaskAsync(task));
    }

    public async Task UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request)
    {
        if (!Enum.TryParse<Enums.TaskStatus>(request.Status, false, out _))
            throw new MancoValidationException($"Invalid task status '{request.Status}'");

        var task = await _repository.GetTaskByIdAsync(taskId)
            ?? throw new NotFoundException($"Task {taskId} not found");

        task.Status = request.Status;
        if (request.Status == nameof(Enums.TaskStatus.Done) && task.CompletedAt is null)
            task.CompletedAt = DateTime.UtcNow;

        task.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task<List<BacklogReasonResponse>> GetBacklogReasonsAsync(Guid projectId) =>
        (await _repository.GetBacklogReasonsAsync(projectId)).Select(MapBacklogReason).ToList();

    public async Task<BacklogReasonResponse> AddBacklogReasonAsync(Guid projectId, AddBacklogReasonRequest request, string callerAzureAdObjectId)
    {
        var caller = await _userResolver.ResolveActiveUserAsync(callerAzureAdObjectId);
        var reason = new ProjectBacklogReason
        {
            ProjectId = projectId,
            ReasonText = request.ReasonText,
            AddedBy = caller.UserId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddBacklogReasonAsync(reason);
        created.AddedByUser = caller;
        return MapBacklogReason(created);
    }

    public async Task<List<PriorityDecisionResponse>> GetPriorityHistoryAsync(Guid projectId) =>
        (await _repository.GetPriorityHistoryAsync(projectId)).Select(MapPriorityDecision).ToList();

    public async Task<PriorityDecisionResponse> SetPriorityAsync(Guid projectId, SetPriorityRequest request, string callerAzureAdObjectId)
    {
        if (!Enum.TryParse<ProjectPriority>(request.PriorityLevel, true, out var parsedPriority))
            throw new MancoValidationException($"Invalid priority level '{request.PriorityLevel}'");

        if (parsedPriority == ProjectPriority.Unset)
            throw new MancoValidationException("PriorityLevel must not be Unset.");

        var caller = await _userResolver.ResolveActiveUserAsync(callerAzureAdObjectId);
        var project = await _repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException($"Project {projectId} not found");

        var priorityLevel = parsedPriority.ToString();
        var decision = new PriorityDecision
        {
            ProjectId = projectId,
            PriorityLevel = priorityLevel,
            Justification = request.Justification,
            DecidedBy = caller.UserId,
            DecidedAt = DateTime.UtcNow,
            ReportId = request.ReportId
        };

        project.CurrentPriority = priorityLevel;
        project.UpdatedAt = DateTime.UtcNow;
        await _repository.AddPriorityDecisionAndUpdateProjectAsync(decision, project);
        decision.DecidedByUser = caller;
        return MapPriorityDecision(decision);
    }

    private static ProjectSummaryResponse MapSummary(ActiveProjectSummaryView view) => new(
        view.ProjectId, view.Title, view.Description, view.Status, view.CurrentPriority, view.StartDate,
        view.TargetDate, view.OwnerId, view.OwnerName, view.OwnerEmail, view.LatestReportStatus,
        view.OpenTaskCount, view.TotalTaskCount, view.UnresolvedCommentCount, view.CreatedAt, view.UpdatedAt);

    private static ProjectResponse MapProject(MancoProject project) => new(
        project.ProjectId, project.Title, project.Description, project.Status, project.CurrentPriority,
        project.StartDate, project.TargetDate, project.OwnerId, project.Owner.DisplayName,
        project.CreatedAt, project.UpdatedAt);

    private static TaskResponse MapTask(MancoTask task) => new(
        task.TaskId, task.ProjectId, task.Title, task.Description, task.Status, task.Priority,
        task.DueDate, task.AssignedTo, task.AssignedUser?.DisplayName, task.CreatedAt, task.UpdatedAt, task.CompletedAt);

    private static BacklogReasonResponse MapBacklogReason(ProjectBacklogReason reason) => new(
        reason.ReasonId, reason.ProjectId, reason.ReasonText, reason.AddedBy,
        reason.AddedByUser.DisplayName, reason.CreatedAt);

    private static PriorityDecisionResponse MapPriorityDecision(PriorityDecision decision) => new(
        decision.DecisionId, decision.ProjectId, decision.PriorityLevel, decision.Justification,
        decision.DecidedBy, decision.DecidedByUser.DisplayName, decision.DecidedAt, decision.ReportId);
}
