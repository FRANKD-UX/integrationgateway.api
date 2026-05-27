using IntegrationGateway.Api.Modules.IncidentWorkflow.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Modules.IncidentWorkflow;

[ApiController]
[Route("api/incidents")]
public class IncidentWorkflowController : ControllerBase
{
    private readonly IncidentWorkflowService _service;
    private readonly ILogger<IncidentWorkflowController> _logger;

    public IncidentWorkflowController(
        IncidentWorkflowService service,
        ILogger<IncidentWorkflowController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetByDepartment(
        [FromQuery] int departmentId,
        [FromQuery] string? status = null)
    {
        var incidents = await _service
            .GetByDepartmentAsync(departmentId, status);

        return Ok(incidents);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var incident = await _service.GetByIdAsync(id);

        if (incident is null)
            return NotFound(new { message = $"Incident {id} not found" });

        return Ok(incident);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateIncidentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.IncidentId },
            created);
    }

    [HttpPatch("{id:int}/checklist/{checklistId:int}")]
    public async Task<IActionResult> CompleteChecklistItem(
        int id,
        int checklistId,
        [FromBody] CompleteChecklistItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CompleteChecklistItemAsync(
            id, checklistId, request);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Incident);
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(
        int id,
        [FromQuery] int userId,
        [FromQuery] int departmentId)
    {
        var result = await _service.CloseIncidentAsync(
            id, userId, departmentId);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Incident);
    }
}