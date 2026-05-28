namespace IntegrationGateway.Api.Modules.Dashboard.Models;

public class DashboardSummaryDto
{
    public List<DashboardKpiDto> Kpis { get; set; } = new();
    public List<DepartmentWorkloadDto> WorkloadByDepartment { get; set; } = new();
    public IncidentTrendsDto Trends { get; set; } = new();
    public List<RecentIncidentDto> RecentIncidents { get; set; } = new();
    public SlaComplianceDto SlaCompliance { get; set; } = new();
}

public class DashboardKpiDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Change { get; set; }
    public string ChangeType { get; set; } = "NEUTRAL";
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class DepartmentWorkloadDto
{
    public DepartmentDto Department { get; set; } = new();
    public int OpenIncidents { get; set; }
    public int InProgress { get; set; }
    public int Escalated { get; set; }
    public double AvgResolutionTime { get; set; }
    public double SlaCompliance { get; set; }
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class IncidentTrendsDto
{
    public List<TrendDataDto> Daily { get; set; } = new();
    public List<TrendDataDto> Weekly { get; set; } = new();
    public List<TrendDataDto> Monthly { get; set; } = new();
}

public class TrendDataDto
{
    public string Date { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Resolved { get; set; }
    public int Escalated { get; set; }
}

public class SlaComplianceDto
{
    public double Overall { get; set; }
    public List<DepartmentSlaDto> ByDepartment { get; set; } = new();
    public List<PrioritySlaDto> ByPriority { get; set; } = new();
}

public class DepartmentSlaDto
{
    public string DepartmentId { get; set; } = string.Empty;
    public double Compliance { get; set; }
}

public class PrioritySlaDto
{
    public string Priority { get; set; } = string.Empty;
    public double Compliance { get; set; }
}

public class RecentIncidentDto
{
    public int IncidentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public string? CurrentDepartmentName { get; set; }
    public DateTime CreatedAt { get; set; }
}