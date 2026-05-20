using IntegrationGateway.Api.Infrastructure.Data.Entities;
using IntegrationGateway.Api.Modules.WorkItems.Models;

namespace IntegrationGateway.Api.Modules.WorkItems;

public class WorkItemService
{
    private readonly WorkItemRepository _repository;
    private readonly ILogger<WorkItemService> _logger;

    public WorkItemService(WorkItemRepository repository, ILogger<WorkItemService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<WorkItemDto>> GetAllAsync(
        int? projectId = null,
        string? status = null,
        string? requestType = null,
        int? departmentId = null)
    {
        var workItems = await _repository.GetAllAsync(projectId, status, requestType, departmentId);
        return workItems.Select(MapToDto);
    }

    public async Task<WorkItemDto?> GetByIdAsync(int workItemId)
    {
        var workItem = await _repository.GetByIdAsync(workItemId);
        return workItem is null ? null : MapToDto(workItem);
    }

    public async Task<WorkItemDto> CreateAsync(CreateWorkItemRequest request)
    {
        var workItem = new WorkItem
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            RequestType = request.RequestType,
            Type = request.Type,
            DepartmentId = request.DepartmentId,
            SiteId = request.SiteId,
            IncidentTypeId = request.IncidentTypeId,
            AssignedToUserId = request.AssignedToUserId,
            CreatedByUserId = request.CreatedByUserId,
            Priority = request.Priority,
            Severity = request.Severity,
            Description = request.Description,
            AffectedService = request.AffectedService,
            Impact = request.Impact,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            Status = "New",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var created = await _repository.CreateAsync(workItem);

        _logger.LogInformation(
            "WorkItem created: {WorkItemId} - {Title}",
            created.WorkItemId,
            created.Title
        );

        return MapToDto(created);
    }

    public async Task<WorkItemDto?> UpdateAsync(int workItemId, UpdateWorkItemRequest request)
    {
        var updated = await _repository.UpdateAsync(workItemId, workItem =>
        {
            if (request.Title is not null) workItem.Title = request.Title;
            if (request.Status is not null) workItem.Status = request.Status;
            if (request.Priority is not null) workItem.Priority = request.Priority;
            if (request.Severity is not null) workItem.Severity = request.Severity;
            if (request.Description is not null) workItem.Description = request.Description;
            if (request.AffectedService is not null) workItem.AffectedService = request.AffectedService;
            if (request.Impact is not null) workItem.Impact = request.Impact;
            if (request.AssignedToUserId.HasValue) workItem.AssignedToUserId = request.AssignedToUserId;
            if (request.SiteId.HasValue) workItem.SiteId = request.SiteId;
            if (request.StartDate.HasValue) workItem.StartDate = request.StartDate;
            if (request.DueDate.HasValue) workItem.DueDate = request.DueDate;
        });

        return updated is null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int workItemId)
    {
        return await _repository.DeleteAsync(workItemId);
    }

    // Maps a database entity to a DTO
    // This is the only place in the codebase that knows about both shapes
    private static WorkItemDto MapToDto(WorkItem w) => new()
    {
        WorkItemId = w.WorkItemId,
        ProjectId = w.ProjectId,
        Title = w.Title,
        Status = w.Status,
        Priority = w.Priority,
        RequestType = w.RequestType,
        Type = w.Type,
        Severity = w.Severity,
        AffectedService = w.AffectedService,
        Impact = w.Impact,
        Description = w.Description,
        DepartmentId = w.DepartmentId,
        DepartmentName = w.Department?.DepartmentName,
        SiteId = w.SiteId,
        SiteName = w.Site?.SiteName,
        AssignedToUserId = w.AssignedToUserId,
        AssignedToUserName = w.AssignedToUser?.DisplayName,
        CreatedByUserId = w.CreatedByUserId,
        CreatedByUserName = w.CreatedByUser?.DisplayName,
        StartDate = w.StartDate,
        DueDate = w.DueDate,
        SladeadLine = w.Sladeadline,
        Slastatus = w.Slastatus,
        SlaresponseMinutes = w.SlaresponseMinutes,
        SlaresolutionMinutes = w.SlaresolutionMinutes,
        ResponseDueDate = w.ResponseDueDate,
        ResolutionDueDate = w.ResolutionDueDate,
        EscalationLevel = w.EscalationLevel,
        LastEscalatedAt = w.LastEscalatedAt,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt
    };
}