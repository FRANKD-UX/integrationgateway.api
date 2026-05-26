namespace IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

public class IncidentDto
{
    public int IncidentId { get; set; }
    public int ProjectId { get; set; }
    public int IncidentTypeId { get; set; }
    public string? IncidentTypeName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public int OriginDepartmentId { get; set; }
    public string? OriginDepartmentName { get; set; }
    public int CurrentDepartmentId { get; set; }
    public string? CurrentDepartmentName { get; set; }
    public int CurrentChainStepOrder { get; set; }
    public int TotalChainSteps { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsReturnedToOrigin { get; set; }
    public bool CanClose { get; set; }
    public List<IncidentChainStepDto> Chain { get; set; } = new();
}