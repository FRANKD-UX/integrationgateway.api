using IntegrationGateway.Api.Modules.WorkItems.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Modules.WorkItems;

[ApiController]
[Route("api/[controller]")]
public class WorkItemsController : ControllerBase
{
    private readonly WorkItemService _service;
    private readonly ILogger<WorkItemsController> _logger;

    public WorkItemsController(WorkItemService service, ILogger<WorkItemsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? projectId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? requestType = null,
        [FromQuery] int? departmentId = null)
    {
        var workItems = await _service.GetAllAsync(projectId, status, requestType, departmentId);
        return Ok(workItems);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var workItem = await _service.GetByIdAsync(id);

        if (workItem is null)
            return NotFound(new { message = $"WorkItem {id} not found" });

        return Ok(workItem);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.WorkItemId }, created);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _service.UpdateAsync(id, request);

        if (updated is null)
            return NotFound(new { message = $"WorkItem {id} not found" });

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = $"WorkItem {id} not found" });

        return NoContent();
    }
}