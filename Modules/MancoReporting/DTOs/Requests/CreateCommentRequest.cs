using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record CreateCommentRequest : IValidatableObject
{
    public Guid? ReportId { get; init; }

    public Guid? ProjectId { get; init; }

    [Required]
    public string CommentText { get; init; } = null!;

    public string CommentType { get; init; } = "General";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ReportId.HasValue == ProjectId.HasValue)
            yield return new ValidationResult(
                "Exactly one of ReportId or ProjectId must be provided.",
                [nameof(ReportId), nameof(ProjectId)]);
    }
}
