using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Modules.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly CurrentUserService _currentUserService;
    private readonly PermissionService _permissionService;

    public AuthController(CurrentUserService currentUserService, PermissionService permissionService)
    {
        _currentUserService = currentUserService;
        _permissionService = permissionService;
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        return Ok(await _currentUserService.GetCurrentUserAsync(HttpContext.User));
    }

    [Authorize]
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Permissions()
    {
        return Ok(_permissionService.GetPermissions(HttpContext.User));
    }
}
