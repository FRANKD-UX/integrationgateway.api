using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record SetPriorityRequest : IValidatableObject
{
    [Required]
    public string PriorityLevel { get; init; } = null!;

    public string? Justification { get; init; }

    public Guid? ReportId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(PriorityLevel, "Unset", StringComparison.OrdinalIgnoreCase))
            yield return new ValidationResult(
                "PriorityLevel must not be Unset.",
                [nameof(PriorityLevel)]);
    }
}
