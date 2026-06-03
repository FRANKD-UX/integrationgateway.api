using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record CreateCommentRequest(
    Guid? ReportId,
    Guid? ProjectId,
    [property: Required] string CommentText,
    string CommentType = "General") : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ReportId.HasValue == ProjectId.HasValue)
            yield return new ValidationResult(
                "Exactly one of ReportId or ProjectId must be provided.",
                [nameof(ReportId), nameof(ProjectId)]);
    }
}
