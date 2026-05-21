using IntegrationGateway.Api.Modules.WorkItems;
using IntegrationGateway.Api.Modules.WorkItems.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Modules.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly WorkItemService _service;
    private readonly ILogger<TasksController> _logger;

    // Tasks are WorkItems with RequestType = "Task"
    private const string TaskRequestType = "Task";

    public TasksController(WorkItemService service, ILogger<TasksController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? projectId = null,
        [FromQuery] string? status = null,
        [FromQuery] int? departmentId = null)
    {
        var tasks = await _service.GetAllAsync(
            projectId: projectId,
            status: status,
            requestType: TaskRequestType,
            departmentId: departmentId
        );

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id);

        if (task is null)
            return NotFound(new { message = $"Task {id} not found" });

        if (task.RequestType != TaskRequestType)
            return NotFound(new { message = $"Task {id} not found" });

        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Force RequestType to Task regardless of what caller sends
        request.RequestType = TaskRequestType;

        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.WorkItemId }, created);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(id);

        if (existing is null || existing.RequestType != TaskRequestType)
            return NotFound(new { message = $"Task {id} not found" });

        var updated = await _service.UpdateAsync(id, request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);

        if (existing is null || existing.RequestType != TaskRequestType)
            return NotFound(new { message = $"Task {id} not found" });

        await _service.DeleteAsync(id);
        return NoContent();
    }
}