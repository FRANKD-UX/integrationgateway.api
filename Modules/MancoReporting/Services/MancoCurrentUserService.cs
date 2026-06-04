using System.Security.Claims;
using IntegrationGateway.Api.Modules.MancoReporting.Data;
using IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;
using IntegrationGateway.Api.Modules.MancoReporting.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.MancoReporting.Services;

public sealed class MancoCurrentUserService : IMancoCurrentUserService
{
    private const string ObjectIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    private readonly MancoDbContext _context;
    private readonly ILogger<MancoCurrentUserService> _logger;

    public MancoCurrentUserService(MancoDbContext context, ILogger<MancoCurrentUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MancoCurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        var azureAdObjectId = ResolveAzureAdObjectId(user);

        _logger.LogInformation("Resolved Azure AD object id {AzureAdObjectId} for Manco current-user lookup.", azureAdObjectId);

        var mancoUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.AzureAdObjectId == azureAdObjectId);

        if (mancoUser is null)
        {
            _logger.LogWarning("No Manco user profile matched Azure AD object id {AzureAdObjectId}.", azureAdObjectId);
            throw new NotFoundException("No active Manco user profile found for this Azure AD object ID.");
        }

        _logger.LogInformation(
            "Matched Manco user {MancoUserId} for Azure AD object id {AzureAdObjectId}. Active: {IsActive}.",
            mancoUser.UserId,
            azureAdObjectId,
            mancoUser.IsActive);

        if (!mancoUser.IsActive)
        {
            throw new UnauthorizedAccessException("Manco user profile is inactive.");
        }

        return new MancoCurrentUserDto(
            mancoUser.UserId,
            mancoUser.AzureAdObjectId,
            mancoUser.DisplayName,
            mancoUser.Email,
            mancoUser.Role,
            mancoUser.IsActive,
            MapCapabilities(mancoUser.Role));
    }

    private static string ResolveAzureAdObjectId(ClaimsPrincipal user)
    {
        var azureAdObjectId = user.FindFirst("oid")?.Value
            ?? user.FindFirst(ObjectIdentifierClaimType)?.Value;

        if (string.IsNullOrWhiteSpace(azureAdObjectId))
        {
            throw new UnauthorizedAccessException(
                $"Authenticated user token does not contain an Azure AD object id claim. Expected 'oid' or '{ObjectIdentifierClaimType}'.");
        }

        return azureAdObjectId;
    }

    private static MancoCapabilitiesDto MapCapabilities(string role)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return new MancoCapabilitiesDto(
                CanCreateProject: true,
                CanSubmitReport: true,
                CanAddBacklogReason: true,
                CanUpdateTaskStatus: true,
                CanReadComments: true,
                CanReadPriorityHistory: true,
                CanCreateComment: true,
                CanResolveComment: true,
                CanSetPriority: true,
                CanReviewReport: true,
                CanActionReport: true,
                CanReadAllProjects: true);
        }

        if (string.Equals(role, "Manco", StringComparison.OrdinalIgnoreCase))
        {
            return new MancoCapabilitiesDto(
                CanCreateProject: false,
                CanSubmitReport: false,
                CanAddBacklogReason: false,
                CanUpdateTaskStatus: false,
                CanReadComments: true,
                CanReadPriorityHistory: true,
                CanCreateComment: true,
                CanResolveComment: true,
                CanSetPriority: true,
                CanReviewReport: true,
                CanActionReport: true,
                CanReadAllProjects: true);
        }

        if (string.Equals(role, "TeamMember", StringComparison.OrdinalIgnoreCase))
        {
            return new MancoCapabilitiesDto(
                CanCreateProject: true,
                CanSubmitReport: true,
                CanAddBacklogReason: true,
                CanUpdateTaskStatus: true,
                CanReadComments: true,
                CanReadPriorityHistory: true,
                CanCreateComment: false,
                CanResolveComment: false,
                CanSetPriority: false,
                CanReviewReport: false,
                CanActionReport: false,
                CanReadAllProjects: false);
        }

        return new MancoCapabilitiesDto(
            CanCreateProject: false,
            CanSubmitReport: false,
            CanAddBacklogReason: false,
            CanUpdateTaskStatus: false,
            CanReadComments: false,
            CanReadPriorityHistory: false,
            CanCreateComment: false,
            CanResolveComment: false,
            CanSetPriority: false,
            CanReviewReport: false,
            CanActionReport: false,
            CanReadAllProjects: false);
    }
}
