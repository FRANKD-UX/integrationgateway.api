using System.Security.Claims;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using IntegrationGateway.Api.Modules.MancoReporting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Controllers.Manco;

[ApiController]
[Authorize]
[Route("api/manco/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectsController(IProjectService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary() => Ok(await _service.GetSummaryAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request, GetCallerOid());
            return CreatedAtAction(nameof(GetById), new { id = created.ProjectId }, created);
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            return Ok(await _service.GetByIdAsync(id));
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateProjectStatusRequest request)
    {
        try
        {
            await _service.UpdateStatusAsync(id, request, GetCallerOid());
            return NoContent();
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpGet("{id:guid}/tasks")]
    public async Task<IActionResult> GetTasks(Guid id) => Ok(await _service.GetTasksAsync(id));

    [HttpPost("{id:guid}/tasks")]
    public async Task<IActionResult> CreateTask(Guid id, CreateTaskRequest request)
    {
        try
        {
            var created = await _service.CreateTaskAsync(id, request, GetCallerOid());
            return CreatedAtAction(nameof(GetTasks), new { id }, created);
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpPatch("{id:guid}/tasks/{taskId:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, Guid taskId, UpdateTaskStatusRequest request)
    {
        try
        {
            await _service.UpdateTaskStatusAsync(taskId, request);
            return NoContent();
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpGet("{id:guid}/backlog-reasons")]
    public async Task<IActionResult> GetBacklogReasons(Guid id) => Ok(await _service.GetBacklogReasonsAsync(id));

    [HttpPost("{id:guid}/backlog-reasons")]
    public async Task<IActionResult> AddBacklogReason(Guid id, AddBacklogReasonRequest request)
    {
        try
        {
            var created = await _service.AddBacklogReasonAsync(id, request, GetCallerOid());
            return CreatedAtAction(nameof(GetBacklogReasons), new { id }, created);
        }
        catch (Exception ex) when (TryMapException(ex, out var result))
        {
            return result;
        }
    }

    [HttpGet("{id:guid}/priority-history")]
    public async Task<IActionResult> GetPriorityHistory(Guid id) => Ok(await _service.GetPriorityHistoryAsync(id));

    [HttpPost("{id:guid}/priority")]
    [Authorize(Roles = "Manco,Admin")]
    public async Task<IActionResult> SetPriority(Guid id, SetPriorityRequest request)
    {
        try
        {
            var created = await _service.SetPriorityAsync(id, request, GetCallerOid());
            return CreatedAtAction(nameof(GetPriorityHistory), new { id }, created);
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
