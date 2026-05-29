using Microsoft.AspNetCore.Mvc;
using IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

namespace IntegrationGateway.Api.Modules.IncidentWorkflow;

[ApiController]
[Route("api/incidents")]
public class IncidentWorkflowController : ControllerBase
{
    private readonly IncidentWorkflowService _service;

    public IncidentWorkflowController(IncidentWorkflowService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetIncidentsQuery query)
    {
        var result = await _service.GetAllAsync(query);
        return Ok(result);
    }

    [HttpGet("{incidentId:int}")]
    public async Task<IActionResult> GetById(int incidentId)
    {
        var incident = await _service.GetByIdAsync(incidentId);

        if (incident is null)
            return NotFound();

        return Ok(incident);
    }

    [HttpGet("department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(
        int departmentId,
        [FromQuery] string? status = null)
    {
        var incidents = await _service.GetByDepartmentAsync(departmentId, status);
        return Ok(incidents);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] CreateIncidentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.IncidentId },
                created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{incidentId:int}/checklist/{checklistId:int}/complete")]
    public async Task<IActionResult> CompleteChecklistItem(
        int incidentId,
        int checklistId,
        [FromBody] CompleteChecklistItemRequest request)
    {
        var result = await _service.CompleteChecklistItemAsync(incidentId, checklistId, request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{incidentId:int}/close")]
    public async Task<IActionResult> CloseIncident(
        int incidentId,
        [FromQuery] int requestingUserId,
        [FromQuery] int requestingDepartmentId)
    {
        var result = await _service.CloseIncidentAsync(
            incidentId,
            requestingUserId,
            requestingDepartmentId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

