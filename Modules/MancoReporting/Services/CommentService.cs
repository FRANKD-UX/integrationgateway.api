using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using IntegrationGateway.Api.Modules.MancoReporting.Repositories;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _repository;
    private readonly IMancoUserResolver _userResolver;

    public CommentService(ICommentRepository repository, IMancoUserResolver userResolver)
    {
        _repository = repository;
        _userResolver = userResolver;
    }

    public async Task<List<CommentResponse>> GetByReportAsync(Guid reportId) =>
        (await _repository.GetByReportAsync(reportId)).Select(MapComment).ToList();

    public async Task<List<CommentResponse>> GetByProjectAsync(Guid projectId) =>
        (await _repository.GetByProjectAsync(projectId)).Select(MapComment).ToList();

    public async Task<CommentResponse> CreateAsync(CreateCommentRequest request, string callerAzureAdObjectId)
    {
        var caller = await _userResolver.ResolveActiveUserAsync(callerAzureAdObjectId);
        var comment = new MancoComment
        {
            ReportId = request.ReportId,
            ProjectId = request.ProjectId,
            CommentText = request.CommentText,
            CommentType = request.CommentType,
            AuthorId = caller.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(comment);
        created.Author = caller;
        return MapComment(created);
    }

    public async Task ResolveAsync(Guid commentId, string resolverAzureAdObjectId)
    {
        var resolver = await _userResolver.ResolveActiveUserAsync(resolverAzureAdObjectId);
        var comment = await _repository.GetByIdAsync(commentId)
            ?? throw new NotFoundException($"Comment {commentId} not found");

        comment.IsResolved = true;
        comment.ResolvedBy = resolver.UserId;
        comment.ResolvedAt = DateTime.UtcNow;
        comment.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(comment);
    }

    private static CommentResponse MapComment(MancoComment comment) => new(
        comment.CommentId, comment.ReportId, comment.ProjectId, comment.CommentText, comment.CommentType,
        comment.AuthorId, comment.Author.DisplayName, comment.CreatedAt, comment.IsResolved,
        comment.ResolvedBy, comment.ResolvedAt);
}
