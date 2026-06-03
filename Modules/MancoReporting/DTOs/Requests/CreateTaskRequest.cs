using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record CreateTaskRequest(
    Guid ProjectId,
    [property: Required, MaxLength(256)] string Title,
    string? Description,
    string Priority = "Medium",
    DateOnly? DueDate = null,
    Guid? AssignedTo = null);
