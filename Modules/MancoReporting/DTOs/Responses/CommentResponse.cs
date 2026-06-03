namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record CommentResponse(
    Guid CommentId,
    Guid? ReportId,
    Guid? ProjectId,
    string CommentText,
    string CommentType,
    Guid AuthorId,
    string AuthorName,
    DateTime CreatedAt,
    bool IsResolved,
    Guid? ResolvedBy,
    DateTime? ResolvedAt);
