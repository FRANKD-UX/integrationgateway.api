namespace IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

public class ChecklistItemDto
{
    public int ChecklistId { get; set; }
    public int StepId { get; set; }
    public string ItemText { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public int? CompletedByUserId { get; set; }
    public string? CompletedByUserName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}