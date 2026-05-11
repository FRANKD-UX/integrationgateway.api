using IntegrationGateway.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Controllers
{
    [ApiController]
    [Route("api/collab")]
    public class CollaborationController : ControllerBase
    {
        private readonly SharePointService _sharePointService;

        public CollaborationController(SharePointService sharePointService)
        {
            _sharePointService = sharePointService;
        }

        [HttpPost("request")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateApprovalRequest(
            [FromBody] ApprovalRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(model.RecipientEmail))
                return BadRequest("Recipient email is required.");

            var fields = new Dictionary<string, object>
            {
                ["Title"] = model.Title ?? "Approval Request",
                ["TaskTitle"] = model.Title ?? "Approval Request"
            };

            var token = await _sharePointService.CreateApprovalItemAndNotify(
                model.RecipientEmail, fields);

            if (token == null)
                return StatusCode(500, "Failed to create approval item.");

            return Ok(new { token });
        }

        [HttpGet("respond")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Respond(
            [FromQuery] string token,
            [FromQuery] string action)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(action))
                return BadRequest(BuildHtmlPage("Missing token or action.", isError: true));

            if (action != "accept" && action != "decline")
                return BadRequest(BuildHtmlPage("Invalid action.", isError: true));

            bool success = await _sharePointService.ProcessResponse(token, action);

            if (!success)
                return NotFound(BuildHtmlPage(
                    "This request is invalid or already processed.", isError: true));

            string message = action == "accept"
                ? "✅ Request Accepted"
                : "❌ Request Declined";

            return new ContentResult
            {
                Content = BuildHtmlPage(message),
                ContentType = "text/html",
                StatusCode = 200
            };
        }

        private static string BuildHtmlPage(string message, bool isError = false)
        {
            var color = isError ? "red" : "green";
            return $@"
                <html>
                    <head><title>Approval Response</title></head>
                    <body style='font-family: Arial, sans-serif; text-align: center; padding-top: 50px;'>
                        <h2 style='color: {color};'>{message}</h2>
                        <p>You may close this tab.</p>
                    </body>
                </html>";
        }
    }

    public class ApprovalRequestModel
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}