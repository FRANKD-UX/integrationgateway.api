using IntegrationGateway.Api.Modules.MancoReporting.Data;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using Microsoft.EntityFrameworkCore;
using MancoValidationException = IntegrationGateway.Api.Modules.MancoReporting.Exceptions.ValidationException;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public class MancoUserResolver : IMancoUserResolver
{
    private readonly MancoDbContext _context;

    public MancoUserResolver(MancoDbContext context)
    {
        _context = context;
    }

    public async Task<MancoUser> ResolveActiveUserAsync(string azureAdObjectId)
    {
        if (string.IsNullOrWhiteSpace(azureAdObjectId))
            throw new MancoValidationException("Authenticated user object id was not found in the token.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.AzureAdObjectId == azureAdObjectId)
            ?? throw new NotFoundException($"Manco user with Azure AD object id {azureAdObjectId} not found");

        if (!user.IsActive)
            throw new MancoValidationException("Manco user account is inactive.");

        var now = DateTime.UtcNow;
        if (user.LastLoginAt is null || user.LastLoginAt.Value <= now.AddMinutes(-30))
        {
            user.LastLoginAt = now;
            await _context.SaveChangesAsync();
        }

        return user;
    }
}
