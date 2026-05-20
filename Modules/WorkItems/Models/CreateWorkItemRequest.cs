using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.WorkItems.Models;

public class CreateWorkItemRequest
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string RequestType { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty;

    [Required]
    public int DepartmentId { get; set; }

    public int? SiteId { get; set; }
    public int? IncidentTypeId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? CreatedByUserId { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    [MaxLength(10)]
    public string? Severity { get; set; }

    public string? Description { get; set; }

    [MaxLength(250)]
    public string? AffectedService { get; set; }

    public string? Impact { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
}