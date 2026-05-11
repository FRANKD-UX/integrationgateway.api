// ApiKeyMiddleware.cs
namespace IntegrationGateway.Api.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeader = "X-Api-Key";

        // Only these routes require an API key
        private static readonly string[] ProtectedRoutes =
        [
            "/api/collab/request"
        ];

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration config)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();

            // Only enforce on protected routes
            var isProtected = ProtectedRoutes.Any(r =>
                path != null && path.StartsWith(r, StringComparison.OrdinalIgnoreCase));

            if (isProtected)
            {
                // Check header exists
                if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Missing API key.");
                    return;
                }

                // Compare against configured key
                var validKey = config["App:ApiKey"];

                if (string.IsNullOrWhiteSpace(validKey))
                {
                    // Fail closed — if no key is configured, deny all requests
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("API key is not configured on the server.");
                    return;
                }

                if (!string.Equals(providedKey, validKey, StringComparison.Ordinal))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid API key.");
                    return;
                }
            }

            await _next(context);
        }
    }
}