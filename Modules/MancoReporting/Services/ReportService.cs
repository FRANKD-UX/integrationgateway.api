using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Views;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using IntegrationGateway.Api.Modules.MancoReporting.Repositories;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;
    private readonly IMancoUserResolver _userResolver;

    public ReportService(IReportRepository repository, IMancoUserResolver userResolver)
    {
        _repository = repository;
        _userResolver = userResolver;
    }

    public async Task<List<ReportResponse>> GetByProjectAsync(Guid projectId) =>
        (await _repository.GetByProjectAsync(projectId)).Select(MapReport).ToList();

    public async Task<ReportResponse> GetDetailAsync(Guid reportId)
    {
        var report = await _repository.GetDetailAsync(reportId)
            ?? throw new NotFoundException($"Report {reportId} not found");
        return MapReport(report);
    }

    public async Task<ReportResponse> CreateAsync(CreateReportRequest request, string callerAzureAdObjectId)
    {
        var caller = await _userResolver.ResolveActiveUserAsync(callerAzureAdObjectId);
        if (await _repository.ExistsAsync(request.ProjectId, request.WeekNumber, request.Year))
            throw new ConflictException($"A report for project {request.ProjectId} week {request.WeekNumber}/{request.Year} already exists");

        var report = new WeeklyReport
        {
            ProjectId = request.ProjectId,
            WeekNumber = request.WeekNumber,
            Year = request.Year,
            Summary = request.Summary,
            Achievements = request.Achievements,
            Blockers = request.Blockers,
            PlannedNextWeek = request.PlannedNextWeek,
            Status = "Draft",
            SubmittedBy = caller.UserId
        };

        await _repository.CreateAsync(report);
        return await GetDetailAsync(report.ReportId);
    }

    public async Task SubmitAsync(Guid reportId, string callerAzureAdObjectId)
    {
        _ = await _userResolver.ResolveActiveUserAsync(callerAzureAdObjectId);
        var report = await GetTrackedReportAsync(reportId);
        if (report.Status != "Draft")
            throw new InvalidOperationException("Only draft reports can be submitted.");

        report.Status = "Submitted";
        report.SubmittedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(report);
    }

    public async Task ReviewAsync(Guid reportId, string reviewerAzureAdObjectId)
    {
        var reviewer = await _userResolver.ResolveActiveUserAsync(reviewerAzureAdObjectId);
        var report = await GetTrackedReportAsync(reportId);
        if (report.Status is not ("Submitted" or "UnderReview"))
            throw new InvalidOperationException("Only submitted or under-review reports can be reviewed.");

        report.Status = "UnderReview";
        report.ReviewedBy = reviewer.UserId;
        report.ReviewedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(report);
    }

    public async Task ActionAsync(Guid reportId)
    {
        var report = await GetTrackedReportAsync(reportId);
        report.Status = "Actioned";
        await _repository.UpdateAsync(report);
    }

    private async Task<WeeklyReport> GetTrackedReportAsync(Guid reportId) =>
        await _repository.GetByIdAsync(reportId)
            ?? throw new NotFoundException($"Report {reportId} not found");

    private static ReportResponse MapReport(WeeklyReportDetailView view) => new(
        view.ReportId, view.ProjectId, view.ProjectTitle, view.ProjectStatus, view.CurrentPriority,
        view.WeekNumber, view.Year, view.Summary, view.Achievements, view.Blockers,
        view.PlannedNextWeek, view.ReportStatus, view.SubmittedBy, view.SubmittedByName,
        view.SubmittedByEmail, view.SubmittedAt, view.ReviewedBy, view.ReviewedByName, view.ReviewedAt);
}
