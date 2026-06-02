using System.Security.Claims;
using IntegrationGateway.Api.Infrastructure.Data;
using IntegrationGateway.Api.Modules.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.Auth;

public sealed class ClientApplicationService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ClientApplicationService> _logger;

    public ClientApplicationService(AppDbContext dbContext, ILogger<ClientApplicationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ClientApplicationDto?> GetKnownClientAsync(string? clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return null;

        var client = await TryQueryAsync(() => _dbContext.ClientApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(app => app.ClientId == clientId));

        if (client == null)
            return null;

        return new ClientApplicationDto
        {
            Id = client.Id,
            Name = client.Name,
            ClientId = client.ClientId,
            AppType = client.AppType,
            AuthProvider = client.AuthProvider,
            IsActive = client.IsActive
        };
    }

    public async Task<bool> IsClientAllowedAsync(string? clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return false;

        var hasRegisteredClients = await TryQueryAsync(() => _dbContext.ClientApplications.AnyAsync());
        if (!hasRegisteredClients)
            return true;

        return await TryQueryAsync(() => _dbContext.ClientApplications
            .AsNoTracking()
            .AnyAsync(app => app.ClientId == clientId && app.IsActive));
    }

    public async Task<string> ResolveSourceAppAsync(ClaimsPrincipal user)
    {
        var clientId = user.FindFirst("azp")?.Value ?? user.FindFirst("appid")?.Value;

        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Authenticated principal is missing azp/appid client application claim.");
            return string.Empty;
        }

        var knownClient = await GetKnownClientAsync(clientId);
        var resolved = knownClient?.Name ?? clientId;

        _logger.LogInformation("Resolved client application {ClientId} as {ClientApplication}", clientId, resolved);

        return resolved;
    }

    private async Task<T?> TryQueryAsync<T>(Func<Task<T>> query)
    {
        try
        {
            return await query();
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex.GetType().Namespace?.StartsWith("Microsoft.Data.SqlClient", StringComparison.Ordinal) == true)
        {
            _logger.LogWarning(ex, "Client application registry is unavailable; falling back to token claims.");
            return default;
        }
    }
}
