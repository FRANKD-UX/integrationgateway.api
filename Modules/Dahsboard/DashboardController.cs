using IntegrationGateway.Api.Modules.Dashboard.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Modules.Dashboard;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        DashboardService service,
        ILogger<DashboardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _service.GetSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
    }

    [HttpGet("admin-stats")]
    public async Task<IActionResult> GetAdminStats()
    {
        var stats = await _service.GetAdminStatsAsync();
        return Ok(ApiResponse<AdminDashboardStatsDto>.Ok(stats));
    }
}