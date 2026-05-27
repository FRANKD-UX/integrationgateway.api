using IntegrationGateway.Api.Infrastructure.Data.Entities;
using IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

namespace IntegrationGateway.Api.Modules.IncidentWorkflow;

public class IncidentWorkflowService
{
    private readonly IncidentWorkflowRepository _repository;
    private readonly WorkflowEngine _workflowEngine;
    private readonly NotificationService _notificationService;
    private readonly ILogger<IncidentWorkflowService> _logger;

    public IncidentWorkflowService(
        IncidentWorkflowRepository repository,
        WorkflowEngine workflowEngine,
        NotificationService notificationService,
        ILogger<IncidentWorkflowService> logger)
    {
        _repository = repository;
        _workflowEngine = workflowEngine;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentRequest request)
    {
        var chainTemplate = await _repository
            .GetChainTemplateAsync(request.IncidentTypeId);

        if (!chainTemplate.Any())
            throw new InvalidOperationException(
                $"No chain defined for incident type {request.IncidentTypeId}");

        var now = DateTime.UtcNow;
        var totalSteps = chainTemplate.Count;

        var incident = new Incident
        {
            ProjectId = request.ProjectId,
            IncidentTypeId = request.IncidentTypeId,
            Title = request.Title,
            Description = request.Description,
            Status = "Logged",
            Priority = request.Priority,
            OriginDepartmentId = request.OriginDepartmentId,
            CurrentDepartmentId = request.OriginDepartmentId,
            CurrentChainStepOrder = 1,
            TotalChainSteps = totalSteps,
            CreatedByUserId = request.CreatedByUserId,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        var created = await _repository.CreateIncidentAsync(incident);

        foreach (var templateStep in chainTemplate)
        {
            var isFirstStep = templateStep.StepOrder == 1;

            var step = new IncidentChainStep
            {
                IncidentId = created.IncidentId,
                StepOrder = templateStep.StepOrder,
                DepartmentId = templateStep.DepartmentId,
                Status = isFirstStep ? "Active" : "Pending",
                SlaMinutes = templateStep.SlaMinutes,
                StartedAt = isFirstStep ? now : null,
                DueAt = isFirstStep
                    ? now.AddMinutes(templateStep.SlaMinutes)
                    : null,
                CreatedAt = now
            };

            var createdStep = await _repository.CreateChainStepAsync(step);

            var checklistTemplate = await _repository
                .GetChecklistTemplateAsync(
                    request.IncidentTypeId,
                    templateStep.StepOrder);

            if (checklistTemplate.Any())
            {
                var checklistItems = checklistTemplate
                    .Select(t => new IncidentChainChecklist
                    {
                        StepId = createdStep.StepId,
                        IncidentId = created.IncidentId,
                        ChecklistItemId = t.ChecklistItemId,
                        ItemText = t.ItemText,
                        IsRequired = t.IsRequired,
                        IsCompleted = false,
                        CreatedAt = now
                    })
                    .ToList();

                await _repository.AddChecklistItemsAsync(checklistItems);
            }
        }

        await _notificationService.NotifyDepartmentAsync(
            created.IncidentId,
            request.OriginDepartmentId,
            "A new incident has been assigned to your department",
            NotificationType.NewIncident
        );

        await _repository.SaveAuditLogAsync(new IncidentAuditLog
        {
            IncidentId = created.IncidentId,
            Action = "Created",
            ToStatus = "Logged",
            PerformedByUserId = request.CreatedByUserId,
            PerformedAt = now,
            Notes = $"Incident created with {totalSteps} step chain"
        });

        var result = await _repository
            .GetIncidentWithChainAsync(created.IncidentId);

        return MapToDto(result!);
    }

    public async Task<IncidentDto?> GetByIdAsync(int incidentId)
    {
        var incident = await _repository
            .GetIncidentWithChainAsync(incidentId);

        return incident is null ? null : MapToDto(incident);
    }

    public async Task<IEnumerable<IncidentDto>> GetByDepartmentAsync(
        int departmentId,
        string? status = null)
    {
        var incidents = await _repository
            .GetByDepartmentAsync(departmentId, status);

        return incidents.Select(MapToDto);
    }

    public async Task<PagedResult<IncidentDto>> GetAllAsync(
    GetIncidentsQuery query)
    {
        var (items, total) = await _repository.GetAllAsync(query);

        return new PagedResult<IncidentDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<TransitionResult> CompleteChecklistItemAsync(
        int incidentId,
        int checklistId,
        CompleteChecklistItemRequest request)
    {
        var item = await _repository.GetChecklistItemAsync(checklistId);

        if (item is null)
            return TransitionResult.Fail("Checklist item not found");

        if (item.IncidentId != incidentId)
            return TransitionResult.Fail(
                "Checklist item does not belong to this incident");

        if (item.IsCompleted)
            return TransitionResult.Fail("Checklist item is already completed");

        var now = DateTime.UtcNow;
        item.IsCompleted = true;
        item.CompletedByUserId = request.CompletedByUserId;
        item.CompletedAt = now;
        item.Notes = request.Notes;

        await _repository.SaveChangesAsync();

        var advanced = await _workflowEngine
            .TryAdvanceAsync(incidentId, request.CompletedByUserId);

        if (advanced)
        {
            var updated = await _repository
                .GetIncidentWithChainAsync(incidentId);

            await _notificationService.NotifyDepartmentAsync(
                incidentId,
                updated!.CurrentDepartmentId,
                updated.Status == "ReturnedToOrigin"
                    ? "An incident has been returned to your department for sign off"
                    : "An incident has been handed off to your department",
                updated.Status == "ReturnedToOrigin"
                    ? NotificationType.ReturnedToOrigin
                    : NotificationType.HandedOff
            );

            return TransitionResult.Ok(MapToDto(updated));
        }

        var current = await _repository
            .GetIncidentWithChainAsync(incidentId);

        return TransitionResult.Ok(MapToDto(current!));
    }

    public async Task<TransitionResult> CloseIncidentAsync(
        int incidentId,
        int requestingUserId,
        int requestingDepartmentId)
    {
        var (success, error) = await _workflowEngine
            .TryCloseAsync(
                incidentId,
                requestingUserId,
                requestingDepartmentId);

        if (!success)
            return TransitionResult.Fail(error!);

        var updated = await _repository
            .GetIncidentWithChainAsync(incidentId);

        await _notificationService.NotifyAllInvolvedAsync(
            incidentId,
            "Incident has been closed by the originating department"
        );

        return TransitionResult.Ok(MapToDto(updated!));
    }

    private static IncidentDto MapToDto(Incident i) => new()
    {
        IncidentId = i.IncidentId,
        ProjectId = i.ProjectId,
        IncidentTypeId = i.IncidentTypeId,
        IncidentTypeName = i.IncidentType?.Title,
        Title = i.Title,
        Description = i.Description,
        Status = i.Status,
        Priority = i.Priority,
        OriginDepartmentId = i.OriginDepartmentId,
        OriginDepartmentName = i.OriginDepartment?.DepartmentName,
        CurrentDepartmentId = i.CurrentDepartmentId,
        CurrentDepartmentName = i.CurrentDepartment?.DepartmentName,
        CurrentChainStepOrder = i.CurrentChainStepOrder,
        TotalChainSteps = i.TotalChainSteps,
        CreatedByUserId = i.CreatedByUserId,
        CreatedByUserName = null,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt,
        ClosedAt = i.ClosedAt,
        IsReturnedToOrigin = i.Status == "ReturnedToOrigin",
        CanClose = i.Status == "ReturnedToOrigin",
        Chain = i.IncidentChainSteps
            .OrderBy(s => s.StepOrder)
            .Select(s => new IncidentChainStepDto
            {
                StepId = s.StepId,
                StepOrder = s.StepOrder,
                DepartmentId = s.DepartmentId,
                DepartmentName = s.Department?.DepartmentName,
                Status = s.Status,
                SlaMinutes = s.SlaMinutes,
                StartedAt = s.StartedAt,
                DueAt = s.DueAt,
                CompletedAt = s.CompletedAt,
                IsCurrentStep = s.StepOrder == i.CurrentChainStepOrder,
                ChecklistItems = s.IncidentChainChecklists
                    .Select(c => new ChecklistItemDto
                    {
                        ChecklistId = c.ChecklistId,
                        StepId = c.StepId,
                        ItemText = c.ItemText,
                        IsRequired = c.IsRequired,
                        IsCompleted = c.IsCompleted,
                        CompletedByUserId = c.CompletedByUserId,
                        CompletedAt = c.CompletedAt,
                        Notes = c.Notes
                    })
                    .ToList()
            })
            .ToList()
    };
}