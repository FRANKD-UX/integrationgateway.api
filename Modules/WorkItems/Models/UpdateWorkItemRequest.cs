using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.WorkItems.Models;

public class UpdateWorkItemRequest
{
    [MaxLength(300)]
    public string? Title { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    [MaxLength(10)]
    public string? Severity { get; set; }

    public string? Description { get; set; }

    [MaxLength(250)]
    public string? AffectedService { get; set; }

    public string? Impact { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? SiteId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
}