using System.Security.Claims;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using IntegrationGateway.Api.Modules.MancoReporting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Controllers.Manco;

[ApiController]
[Authorize]
[Route("api/manco/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetByProject(Guid projectId) => Ok(await _service.GetByProjectAsync(projectId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        try
        {
            return Ok(await _service.GetDetailAsync(id));
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReportRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request, GetCallerOid());
            return CreatedAtAction(nameof(GetDetail), new { id = created.ReportId }, created);
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        try
        {
            await _service.SubmitAsync(id, GetCallerOid());
            return NoContent();
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:guid}/review")]
    [Authorize(Roles = "Manco,Admin")]
    public async Task<IActionResult> Review(Guid id)
    {
        try
        {
            await _service.ReviewAsync(id, GetCallerOid());
            return NoContent();
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:guid}/action")]
    [Authorize(Roles = "Manco,Admin")]
    public async Task<IActionResult> Action(Guid id)
    {
        try
        {
            await _service.ActionAsync(id);
            return NoContent();
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    private string GetCallerOid() =>
        User.FindFirstValue("oid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private bool TryMapException(Exception ex, out IActionResult result)
    {
        result = ex switch
        {
            NotFoundException => NotFound(new { message = ex.Message }),
            ConflictException => Conflict(new { message = ex.Message }),
            Modules.MancoReporting.Exceptions.ValidationException => BadRequest(new { message = ex.Message }),
            InvalidOperationException => BadRequest(new { message = ex.Message }),
            _ => null!
        };

        return result is not null;
    }
}
