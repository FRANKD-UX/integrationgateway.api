using IntegrationGateway.Api.Infrastructure.Data.Entities;

namespace IntegrationGateway.Api.Modules.IncidentWorkflow;

public class WorkflowEngine
{
    private readonly IncidentWorkflowRepository _repository;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        IncidentWorkflowRepository repository,
        ILogger<WorkflowEngine> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> TryAdvanceAsync(
        int incidentId,
        int requestingUserId)
    {
        var incident = await _repository
            .GetIncidentWithChainAsync(incidentId);

        if (incident is null)
            return false;

        var currentStep = incident.IncidentChainSteps
            .FirstOrDefault(s =>
                s.StepOrder == incident.CurrentChainStepOrder);

        if (currentStep is null)
            return false;

        var allRequiredComplete = currentStep
            .IncidentChainChecklists
            .Where(c => c.IsRequired)
            .All(c => c.IsCompleted);

        if (!allRequiredComplete)
            return false;

        await AdvanceToNextStepAsync(incident, currentStep);
        return true;
    }

    private async Task AdvanceToNextStepAsync(
        Incident incident,
        IncidentChainStep completedStep)
    {
        var now = DateTime.UtcNow;

        completedStep.Status = "Complete";
        completedStep.CompletedAt = now;

        var nextStepOrder = incident.CurrentChainStepOrder + 1;
        var isLastStep = nextStepOrder > incident.TotalChainSteps;

        if (isLastStep)
        {
            incident.Status = "ReturnedToOrigin";
            incident.CurrentDepartmentId = incident.OriginDepartmentId;
            incident.CurrentChainStepOrder = incident.TotalChainSteps;

            _logger.LogInformation(
                "Incident {IncidentId} returned to origin department {DeptId} for sign off",
                incident.IncidentId,
                incident.OriginDepartmentId
            );
        }
        else
        {
            var nextStep = incident.IncidentChainSteps
                .FirstOrDefault(s => s.StepOrder == nextStepOrder);

            if (nextStep is null)
                return;

            nextStep.Status = "Active";
            nextStep.StartedAt = now;
            nextStep.DueAt = now.AddMinutes(nextStep.SlaMinutes);

            incident.CurrentChainStepOrder = nextStepOrder;
            incident.CurrentDepartmentId = nextStep.DepartmentId;
            incident.Status = "InProgress";

            _logger.LogInformation(
                "Incident {IncidentId} advanced to step {StepOrder} department {DeptId}",
                incident.IncidentId,
                nextStepOrder,
                nextStep.DepartmentId
            );
        }

        incident.UpdatedAt = now;

        await _repository.SaveAuditLogAsync(new IncidentAuditLog
        {
            IncidentId = incident.IncidentId,
            Action = isLastStep ? "ReturnedToOrigin" : "AdvancedToNextStep",
            FromStatus = incident.Status,
            ToStatus = isLastStep ? "ReturnedToOrigin" : "InProgress",
            PerformedAt = now,
            Notes = isLastStep
                ? "All checklist items satisfied. Returned to origin for sign off."
                : $"Advanced to step {nextStepOrder}"
        });

        await _repository.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> TryCloseAsync(
        int incidentId,
        int requestingUserId,
        int requestingDepartmentId)
    {
        var incident = await _repository
            .GetIncidentWithChainAsync(incidentId);

        if (incident is null)
            return (false, "Incident not found");

        if (incident.Status != "ReturnedToOrigin")
            return (false, "Incident must be returned to origin department before closure");

        if (requestingDepartmentId != incident.OriginDepartmentId)
            return (false, "Only the originating department can close this incident");

        var returnStep = incident.IncidentChainSteps
            .FirstOrDefault(s =>
                s.StepOrder == incident.TotalChainSteps);

        if (returnStep is not null)
        {
            var allComplete = returnStep.IncidentChainChecklists
                .Where(c => c.IsRequired)
                .All(c => c.IsCompleted);

            if (!allComplete)
                return (false, "All checklist items must be completed before closing");
        }

        var now = DateTime.UtcNow;
        incident.Status = "Closed";
        incident.ClosedAt = now;
        incident.ClosedByUserId = requestingUserId;
        incident.UpdatedAt = now;

        await _repository.SaveAuditLogAsync(new IncidentAuditLog
        {
            IncidentId = incident.IncidentId,
            Action = "Closed",
            FromStatus = "ReturnedToOrigin",
            ToStatus = "Closed",
            PerformedByUserId = requestingUserId,
            PerformedAt = now,
            Notes = "Incident closed by origin department after client confirmation"
        });

        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Incident {IncidentId} closed by user {UserId}",
            incidentId,
            requestingUserId
        );

        return (true, null);
    }
}