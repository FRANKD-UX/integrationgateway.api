using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

public class CreateIncidentRequest
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int IncidentTypeId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public int OriginDepartmentId { get; set; }

    [Required]
    public int CreatedByUserId { get; set; }

    [MaxLength(10)]
    public string? Priority { get; set; }
}