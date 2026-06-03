using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public interface IMancoUserResolver
{
    Task<MancoUser> ResolveActiveUserAsync(string azureAdObjectId);
}
