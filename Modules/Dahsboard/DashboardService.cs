using IntegrationGateway.Api.Modules.Dashboard.Models;

namespace IntegrationGateway.Api.Modules.Dashboard;

public class DashboardService
{
    private readonly DashboardRepository _repository;

    public DashboardService(DashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminDashboardStatsDto> GetAdminStatsAsync()
    {
        var incidentTypes = await _repository.GetIncidentTypeCountAsync();
        var slaRules = await _repository.GetSlaRuleCountAsync();
        var roles = await _repository.GetRoleCountAsync();

        return new AdminDashboardStatsDto
        {
            IncidentTypes = incidentTypes,
            Workflows = incidentTypes,
            SlaRules = slaRules,
            Roles = roles
        };
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var totalOpen = await _repository.GetTotalOpenAsync();
        var inProgress = await _repository.GetTotalByStatusAsync("InProgress");
        var escalated = await _repository.GetTotalByStatusAsync("Escalated");
        var p1Open = await _repository.GetTotalByPriorityAsync("P1");

        var kpis = new List<DashboardKpiDto>
        {
            new()
            {
                Id = "total-open",
                Label = "Open Incidents",
                Value = totalOpen,
                Change = 0,
                ChangeType = "NEUTRAL",
                Icon = "folder_open",
                Color = "#0078d4"
            },
            new()
            {
                Id = "in-progress",
                Label = "In Progress",
                Value = inProgress,
                Change = 0,
                ChangeType = "NEUTRAL",
                Icon = "pending",
                Color = "#f7a700"
            },
            new()
            {
                Id = "escalated",
                Label = "Escalated",
                Value = escalated,
                Change = 0,
                ChangeType = escalated > 0 ? "INCREASE" : "NEUTRAL",
                Icon = "warning",
                Color = "#d83b01"
            },
            new()
            {
                Id = "p1-open",
                Label = "P1 Incidents",
                Value = p1Open,
                Change = 0,
                ChangeType = p1Open > 0 ? "INCREASE" : "NEUTRAL",
                Icon = "priority_high",
                Color = "#a80000"
            }
        };

        var workload = await _repository.GetDepartmentWorkloadAsync();
        var recentIncidents = await _repository.GetRecentIncidentsAsync();
        var dailyTrends = await _repository.GetDailyTrendsAsync();

        return new DashboardSummaryDto
        {
            Kpis = kpis,
            WorkloadByDepartment = workload,
            RecentIncidents = recentIncidents,
            Trends = new IncidentTrendsDto
            {
                Daily = dailyTrends,
                Weekly = new List<TrendDataDto>(),
                Monthly = new List<TrendDataDto>()
            },
            SlaCompliance = new SlaComplianceDto
            {
                Overall = 0,
                ByDepartment = new List<DepartmentSlaDto>(),
                ByPriority = new List<PrioritySlaDto>()
            }
        };
    }
}