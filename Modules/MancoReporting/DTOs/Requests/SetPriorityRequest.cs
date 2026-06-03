using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record SetPriorityRequest(
    [property: Required] string PriorityLevel,
    string? Justification,
    Guid? ReportId) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.Equals(PriorityLevel, "Unset", StringComparison.OrdinalIgnoreCase))
            yield return new ValidationResult(
                "PriorityLevel must not be Unset.",
                [nameof(PriorityLevel)]);
    }
}
