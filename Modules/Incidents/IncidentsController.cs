using IntegrationGateway.Api.Modules.WorkItems;
using IntegrationGateway.Api.Modules.WorkItems.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Modules.Incidents;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly WorkItemService _service;
    private readonly ILogger<IncidentsController> _logger;

    // Incidents are WorkItems with RequestType = "Incident"
    private const string IncidentRequestType = "Incident";

    public IncidentsController(WorkItemService service, ILogger<IncidentsController> logger)
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
        var incidents = await _service.GetAllAsync(
            projectId: projectId,
            status: status,
            requestType: IncidentRequestType,
            departmentId: departmentId
        );

        return Ok(incidents);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var incident = await _service.GetByIdAsync(id);

        if (incident is null)
            return NotFound(new { message = $"Incident {id} not found" });

        if (incident.RequestType != IncidentRequestType)
            return NotFound(new { message = $"Incident {id} not found" });

        return Ok(incident);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Force RequestType to Incident regardless of what caller sends
        request.RequestType = IncidentRequestType;

        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.WorkItemId }, created);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(id);

        if (existing is null || existing.RequestType != IncidentRequestType)
            return NotFound(new { message = $"Incident {id} not found" });

        var updated = await _service.UpdateAsync(id, request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);

        if (existing is null || existing.RequestType != IncidentRequestType)
            return NotFound(new { message = $"Incident {id} not found" });

        await _service.DeleteAsync(id);
        return NoContent();
    }
}