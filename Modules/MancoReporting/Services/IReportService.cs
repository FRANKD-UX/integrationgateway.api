using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public interface IReportService
{
    Task<List<ReportResponse>> GetByProjectAsync(Guid projectId);
    Task<ReportResponse> GetDetailAsync(Guid reportId);
    Task<ReportResponse> CreateAsync(CreateReportRequest request, string callerAzureAdObjectId);
    Task SubmitAsync(Guid reportId, string callerAzureAdObjectId);
    Task ReviewAsync(Guid reportId, string reviewerAzureAdObjectId);
    Task ActionAsync(Guid reportId);
}
