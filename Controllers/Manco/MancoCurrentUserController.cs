using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using IntegrationGateway.Api.Modules.MancoReporting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Controllers.Manco;

[Authorize]
[ApiController]
[Route("api/manco")]
public class MancoCurrentUserController : ControllerBase
{
    private readonly IMancoCurrentUserService _currentUserService;

    public MancoCurrentUserController(IMancoCurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(MancoCurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MancoCurrentUserDto>> GetMe()
    {
        try
        {
            return Ok(await _currentUserService.GetCurrentUserAsync(User));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex) when (ex.Message == "Manco user profile is inactive.")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
