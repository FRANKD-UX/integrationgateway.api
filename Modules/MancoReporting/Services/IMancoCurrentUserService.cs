using System.Security.Claims;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public interface IMancoCurrentUserService
{
    Task<MancoCurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal user);
}
