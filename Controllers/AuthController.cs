using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Me()
        {
            var user = HttpContext.User;

            var userId = user.FindFirstValue("oid")
                ?? user.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? string.Empty;

            var displayName = user.FindFirstValue("name")
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? string.Empty;

            var email = user.FindFirstValue("preferred_username")
                ?? user.FindFirstValue("email")
                ?? user.FindFirstValue(ClaimTypes.Email)
                ?? user.FindFirstValue("upn")
                ?? string.Empty;

            var roles = user.Claims
                .Where(claim => claim.Type == "roles" || claim.Type == ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Ok(new
            {
                userId,
                displayName,
                email,
                roles
            });
        }
    }
}
