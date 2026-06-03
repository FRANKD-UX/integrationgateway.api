using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public interface ICommentService
{
    Task<List<CommentResponse>> GetByReportAsync(Guid reportId);
    Task<List<CommentResponse>> GetByProjectAsync(Guid projectId);
    Task<CommentResponse> CreateAsync(CreateCommentRequest request, string callerAzureAdObjectId);
    Task ResolveAsync(Guid commentId, string resolverAzureAdObjectId);
}
