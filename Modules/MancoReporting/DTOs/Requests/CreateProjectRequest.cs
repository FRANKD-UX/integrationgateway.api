using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record CreateProjectRequest(
    [property: Required, MaxLength(256)] string Title,
    string? Description,
    DateOnly? StartDate,
    DateOnly? TargetDate,
    Guid OwnerId);
