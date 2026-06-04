namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record MancoCurrentUserDto(
    Guid UserId,
    string AzureAdObjectId,
    string DisplayName,
    string Email,
    string Role,
    bool IsActive,
    MancoCapabilitiesDto Capabilities);
