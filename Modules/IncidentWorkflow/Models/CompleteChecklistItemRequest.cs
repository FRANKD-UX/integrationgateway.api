using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

public class CompleteChecklistItemRequest
{
    [Required]
    public int CompletedByUserId { get; set; }

    public string? Notes { get; set; }
}