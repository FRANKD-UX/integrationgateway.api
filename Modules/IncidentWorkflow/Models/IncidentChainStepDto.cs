namespace IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

public class IncidentChainStepDto
{
    public int StepId { get; set; }
    public int StepOrder { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SlaMinutes { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCurrentStep { get; set; }
    public List<ChecklistItemDto> ChecklistItems { get; set; } = new();
}