using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IntegrationGateway.Api.Services;

namespace IntegrationGateway.Api.Modules.Attachments
{
    public class AttachmentsService
    {
        private const string GraphBaseUrl = "https://graph.microsoft.com/v1.0";
        private const string AttachmentRootFolder = "Incidents";

        private readonly HttpClient _httpClient;
        private readonly GraphAuthService _authService;
        private readonly IConfiguration _config;
        private readonly ILogger<AttachmentsService> _logger;

        public AttachmentsService(
            HttpClient httpClient,
            GraphAuthService authService,
            IConfiguration config,
            ILogger<AttachmentsService> logger)
        {
            _httpClient = httpClient;
            _authService = authService;
            _config = config;
            _logger = logger;
        }

        public async Task<AttachmentUploadResponse> UploadAsync(int incidentId, IFormFile file)
        {
            var accessToken = await _authService.GetAccessTokenAsync();
            var driveBaseUrl = await GetDriveBaseUrlAsync(accessToken);
            var incidentFolderPath = $"{AttachmentRootFolder}/{incidentId}";
            var fileName = SanitizeFileName(file.FileName);

            await EnsureFolderAsync(accessToken, driveBaseUrl, AttachmentRootFolder);
            await EnsureFolderAsync(accessToken, driveBaseUrl, incidentFolderPath);

            var uploadUrl = $"{driveBaseUrl}/root:/{EncodePath(incidentFolderPath)}/{EncodeSegment(fileName)}:/content";
            using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            await using var stream = file.OpenReadStream();
            request.Content = new StreamContent(stream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType);

            using var response = await _httpClient.SendAsync(request);
            var json = await ReadGraphResponseAsync(response, "upload attachment");
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            return new AttachmentUploadResponse(
                GetString(root, "id"),
                GetString(root, "name"),
                GetString(root, "webUrl"),
                GetDateTimeOffset(root, "createdDateTime") ?? DateTimeOffset.UtcNow);
        }

        public async Task<IReadOnlyList<AttachmentListItem>> GetAttachmentsAsync(int incidentId)
        {
            var accessToken = await _authService.GetAccessTokenAsync();
            var driveBaseUrl = await GetDriveBaseUrlAsync(accessToken);
            var folderPath = $"{AttachmentRootFolder}/{incidentId}";
            var listUrl = $"{driveBaseUrl}/root:/{EncodePath(folderPath)}:/children";

            using var request = new HttpRequestMessage(HttpMethod.Get, listUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return [];

            var json = await ReadGraphResponseAsync(response, "get attachments");
            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty("value", out var values))
                return [];

            var attachments = new List<AttachmentListItem>();
            foreach (var item in values.EnumerateArray())
            {
                if (item.TryGetProperty("folder", out _))
                    continue;

                attachments.Add(new AttachmentListItem(
                    GetString(item, "id"),
                    GetString(item, "name"),
                    GetString(item, "webUrl"),
                    GetInt64(item, "size"),
                    GetDateTimeOffset(item, "createdDateTime") ?? DateTimeOffset.UtcNow));
            }

            return attachments;
        }

        public async Task DeleteAsync(int incidentId, string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                throw new AttachmentStorageException("A file id is required.");

            var accessToken = await _authService.GetAccessTokenAsync();
            var driveBaseUrl = await GetDriveBaseUrlAsync(accessToken);
            var deleteUrl = $"{driveBaseUrl}/items/{Uri.EscapeDataString(fileId)}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                return;

            await ReadGraphResponseAsync(response, "delete attachment");
        }

        private async Task<string> GetDriveBaseUrlAsync(string accessToken)
        {
            var driveId = _config["SharePoint:DriveId"];
            if (!string.IsNullOrWhiteSpace(driveId))
                return $"{GraphBaseUrl}/drives/{Uri.EscapeDataString(driveId)}";

            var siteId = await GetSiteIdAsync(accessToken);
            return $"{GraphBaseUrl}/sites/{Uri.EscapeDataString(siteId)}/drive";
        }

        private async Task<string> GetSiteIdAsync(string accessToken)
        {
            var siteId = _config["SharePoint:SiteId"];
            if (!string.IsNullOrWhiteSpace(siteId))
                return siteId;

            var siteUrl = _config["SharePoint:SiteUrl"];
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                throw new AttachmentStorageException(
                    "SharePoint attachment storage is not configured. Set SharePoint:SiteId or SharePoint:SiteUrl.");
            }

            var graphSitePath = BuildGraphSitePath(siteUrl);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{GraphBaseUrl}/sites/{graphSitePath}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var json = await ReadGraphResponseAsync(response, "resolve SharePoint site");
            using var document = JsonDocument.Parse(json);
            return GetString(document.RootElement, "id");
        }

        private async Task EnsureFolderAsync(string accessToken, string driveBaseUrl, string folderPath)
        {
            var getUrl = $"{driveBaseUrl}/root:/{EncodePath(folderPath)}";
            using var getRequest = new HttpRequestMessage(HttpMethod.Get, getUrl);
            getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var getResponse = await _httpClient.SendAsync(getRequest);
            if (getResponse.IsSuccessStatusCode)
                return;

            if (getResponse.StatusCode != HttpStatusCode.NotFound)
            {
                await ReadGraphResponseAsync(getResponse, "check attachment folder");
            }

            var lastSlash = folderPath.LastIndexOf('/');
            var parentPath = lastSlash > -1 ? folderPath[..lastSlash] : string.Empty;
            var folderName = lastSlash > -1 ? folderPath[(lastSlash + 1)..] : folderPath;
            var childrenUrl = string.IsNullOrWhiteSpace(parentPath)
                ? $"{driveBaseUrl}/root/children"
                : $"{driveBaseUrl}/root:/{EncodePath(parentPath)}:/children";

            var body = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["name"] = folderName,
                ["folder"] = new { },
                ["@microsoft.graph.conflictBehavior"] = "fail"
            });

            using var createRequest = new HttpRequestMessage(HttpMethod.Post, childrenUrl);
            createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            createRequest.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var createResponse = await _httpClient.SendAsync(createRequest);
            if (createResponse.IsSuccessStatusCode || createResponse.StatusCode == HttpStatusCode.Conflict)
                return;

            await ReadGraphResponseAsync(createResponse, "create attachment folder");
        }

        private async Task<string> ReadGraphResponseAsync(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return content;

            _logger.LogError(
                "Graph API failed while trying to {Action}. Status: {Status}. Response: {Response}",
                action,
                response.StatusCode,
                content);

            throw new AttachmentStorageException(
                $"Unable to {action.Replace(" attachment", " file")} in SharePoint right now.");
        }

        private static string SanitizeFileName(string fileName)
        {
            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeName))
                throw new AttachmentStorageException("The uploaded file name is invalid.");

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                safeName = safeName.Replace(invalidChar, '_');

            return safeName;
        }

        private static string BuildGraphSitePath(string siteUrl)
        {
            if (!Uri.TryCreate(siteUrl, UriKind.Absolute, out var uri))
                return siteUrl.Trim('/');

            var path = uri.AbsolutePath.Trim('/');
            return string.IsNullOrWhiteSpace(path)
                ? uri.Host
                : $"{uri.Host}:/{path}";
        }

        private static string EncodePath(string path)
        {
            return string.Join('/', path.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(EncodeSegment));
        }

        private static string EncodeSegment(string segment)
        {
            return Uri.EscapeDataString(segment);
        }

        private static string GetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property)
                ? property.GetString() ?? string.Empty
                : string.Empty;
        }

        private static long GetInt64(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) && property.TryGetInt64(out var value)
                ? value
                : 0;
        }

        private static DateTimeOffset? GetDateTimeOffset(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property)
                && property.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(property.GetString(), out var value)
                    ? value
                    : null;
        }
    }

    public record AttachmentUploadResponse(
        string FileId,
        string FileName,
        string FileUrl,
        DateTimeOffset UploadedAt);

    public record AttachmentListItem(
        string FileId,
        string FileName,
        string FileUrl,
        long FileSize,
        DateTimeOffset UploadedAt);

    public class AttachmentStorageException : Exception
    {
        public AttachmentStorageException(string message) : base(message)
        {
        }
    }
}
