// SharePointService.cs
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IntegrationGateway.Api.Services
{
    public class SharePointService
    {
        private readonly HttpClient _httpClient;
        private readonly GraphAuthService _authService;
        private readonly IConfiguration _config;
        private readonly ILogger<SharePointService> _logger;

        public SharePointService(
            HttpClient httpClient,
            GraphAuthService authService,
            IConfiguration config,
            ILogger<SharePointService> logger)
        {
            _httpClient = httpClient;
            _authService = authService;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new approval item in the SharePoint list and sends an approval email.
        /// Returns the generated token on success, null otherwise.
        /// </summary>
        public async Task<string?> CreateApprovalItemAndNotify(
            string recipientEmail,
            Dictionary<string, object> additionalFields)
        {
            var accessToken = await _authService.GetAccessTokenAsync();
            var siteUrl = _config["SharePoint:SiteUrl"];
            var listId = _config["SharePoint:ListId"];

            if (string.IsNullOrWhiteSpace(siteUrl) || string.IsNullOrWhiteSpace(listId))
                throw new InvalidOperationException("SharePoint:SiteUrl or SharePoint:ListId missing.");

            var siteId = await ResolveSiteIdAsync(accessToken, siteUrl);
            if (siteId is null) return null;

            var token = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow.ToString("o");

            var fields = new Dictionary<string, object>(additionalFields)
            {
                ["ResponseToken"] = token,
                ["Status"] = "Pending",
                ["RequestedAt"] = now,
            };

            var createUrl = $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/items";
            var body = new { fields };

            var request = new HttpRequestMessage(HttpMethod.Post, createUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Item creation failed: {Status}, {Error}", response.StatusCode, error);
                return null;
            }

            _logger.LogInformation("Approval item created with token: {Token}", token);

            // ✅ Email failure no longer crashes the whole request
            try
            {
                await SendApprovalEmail(recipientEmail, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Email send failed for token {Token} to {Email} — item created but no email sent.",
                    token, recipientEmail);
            }

            return token;
        }

        /// <summary>
        /// Sends an approval email with Accept/Decline links using Microsoft Graph.
        /// </summary>
        public async Task SendApprovalEmail(string toEmail, string token)
        {
            var accessToken = await _authService.GetAccessTokenAsync();
            var senderEmail = _config["Graph:SenderEmail"];
            var baseUrl = _config["App:BaseUrl"];

            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new InvalidOperationException("Graph:SenderEmail is not configured.");

            var acceptUrl = $"{baseUrl}/api/collab/respond?token={token}&action=accept";
            var declineUrl = $"{baseUrl}/api/collab/respond?token={token}&action=decline";

            var emailBody = new
            {
                message = new
                {
                    subject = "Approval Required",
                    body = new
                    {
                        contentType = "HTML",
                        content = $@"
                            <p>You have a pending approval request.</p>
                            <p>
                                <a href='{acceptUrl}'>✅ Accept</a><br/>
                                <a href='{declineUrl}'>❌ Decline</a>
                            </p>"
                    },
                    toRecipients = new[]
                    {
                        new { emailAddress = new { address = toEmail } }
                    }
                },
                saveToSentItems = "true"
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.microsoft.com/v1.0/users/{senderEmail}/sendMail");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(emailBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email: {Status}, {Error}", response.StatusCode, error);
                throw new Exception("Email could not be sent.");
            }

            _logger.LogInformation("Approval email sent to {Email}", toEmail);
        }

        /// <summary>
        /// Processes an approval response (Accept/Decline). Returns true if successful.
        /// </summary>
        public async Task<bool> ProcessResponse(string token, string action)
        {
            var accessToken = await _authService.GetAccessTokenAsync();
            var siteUrl = _config["SharePoint:SiteUrl"];
            var listId = _config["SharePoint:ListId"];

            if (string.IsNullOrWhiteSpace(siteUrl) || string.IsNullOrWhiteSpace(listId))
                throw new InvalidOperationException("SharePoint:SiteUrl or SharePoint:ListId is missing.");

            var siteId = await ResolveSiteIdAsync(accessToken, siteUrl);
            if (siteId is null) return false;

            using var items = await FetchAllItemsAsync(accessToken, siteId, listId);
            if (items is null) return false;

            if (!items.RootElement.TryGetProperty("value", out var itemsArray))
                return false;

            var itemId = FindItemIdByToken(itemsArray, token);
            if (itemId is null) return false;

            return await UpdateItemStatusAsync(accessToken, siteId, listId, itemId, action);
        }

        // -------------------------------------------------------------------
        // Private helper methods
        // -------------------------------------------------------------------

        private async Task<string?> ResolveSiteIdAsync(string accessToken, string siteUrl)
        {
            var url = $"https://graph.microsoft.com/v1.0/sites/{siteUrl}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to resolve site: {Status}, {Error}", response.StatusCode, error);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("id").GetString();
        }

        private async Task<JsonDocument?> FetchAllItemsAsync(string accessToken, string siteId, string listId)
        {
            var allItems = new List<JsonElement>();
            string? nextLink = $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/items?$expand=fields";

            while (!string.IsNullOrEmpty(nextLink))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, nextLink);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch list items: {Status}, {Error}", response.StatusCode, error);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("value", out var values))
                {
                    foreach (var item in values.EnumerateArray())
                    {
                        allItems.Add(item.Clone());
                    }
                }

                nextLink = doc.RootElement.TryGetProperty("@odata.nextLink", out var next)
                    ? next.GetString()
                    : null;
            }

            var combined = JsonSerializer.Serialize(new { value = allItems });
            return JsonDocument.Parse(combined);
        }

        private string? FindItemIdByToken(JsonElement itemsArray, string token)
        {
            foreach (var item in itemsArray.EnumerateArray())
            {
                if (!item.TryGetProperty("fields", out var fields)) continue;

                string? itemToken = null;
                string? status = null;

                if (fields.TryGetProperty("ResponseToken", out var tokenProp))
                    itemToken = tokenProp.GetString();
                if (fields.TryGetProperty("Status", out var statusProp))
                    status = statusProp.GetString();

                if (string.Equals(itemToken, token, StringComparison.Ordinal)
                    && string.Equals(status, "Pending", StringComparison.Ordinal))
                {
                    return item.GetProperty("id").GetString();
                }
            }
            return null;
        }

        private async Task<bool> UpdateItemStatusAsync(
            string accessToken,
            string siteId,
            string listId,
            string itemId,
            string action)
        {
            var newStatus = action.Equals("accept", StringComparison.OrdinalIgnoreCase)
                ? "Accepted"
                : "Declined";

            var now = DateTime.UtcNow.ToString("o");

            var patchBody = new
            {
                fields = new Dictionary<string, object>
                {
                    ["Status"] = newStatus,
                    ["RespondedAt"] = now
                }
            };

            var url = $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/items/{itemId}";
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(patchBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update item status: {Status}, {Error}", response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Item {ItemId} updated to {Status}", itemId, newStatus);
            return true;
        }
    }
}