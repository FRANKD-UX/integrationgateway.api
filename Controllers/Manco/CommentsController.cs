using System.Security.Claims;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using IntegrationGateway.Api.Modules.MancoReporting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Controllers.Manco;

[ApiController]
[Authorize]
[Route("api/manco/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _service;

    public CommentsController(ICommentService service)
    {
        _service = service;
    }

    [HttpGet("report/{reportId:guid}")]
    public async Task<IActionResult> GetByReport(Guid reportId) => Ok(await _service.GetByReportAsync(reportId));

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetByProject(Guid projectId) => Ok(await _service.GetByProjectAsync(projectId));

    [HttpPost]
    [Authorize(Roles = "Manco,Admin")]
    public async Task<IActionResult> Create(CreateCommentRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request, GetCallerOid());
            return CreatedAtAction(
                request.ReportId.HasValue ? nameof(GetByReport) : nameof(GetByProject),
                request.ReportId.HasValue
                    ? new { reportId = request.ReportId.Value }
                    : new { projectId = request.ProjectId!.Value },
                created);
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Manco,Admin")]
    public async Task<IActionResult> Resolve(Guid id)
    {
        try
        {
            await _service.ResolveAsync(id, GetCallerOid());
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
