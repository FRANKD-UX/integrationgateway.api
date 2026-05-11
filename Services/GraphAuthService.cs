using Microsoft.Identity.Client;

namespace IntegrationGateway.Api.Services
{
    public class GraphAuthService
    {
        private readonly IConfiguration _config;

        public GraphAuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var tenantId = _config["Graph:TenantId"];
            var clientId = _config["Graph:ClientId"];
            var clientSecret = _config["Graph:ClientSecret"];

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();

            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var result = await app
                .AcquireTokenForClient(scopes)
                .ExecuteAsync();

            return result.AccessToken;
        }
    }
}