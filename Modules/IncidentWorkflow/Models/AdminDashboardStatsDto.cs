namespace IntegrationGateway.Api.Modules.Dashboard.Models;

public class AdminDashboardStatsDto
{
    public int IncidentTypes { get; set; }
    public int Workflows { get; set; }
    public int SlaRules { get; set; }
    public int Roles { get; set; }
}