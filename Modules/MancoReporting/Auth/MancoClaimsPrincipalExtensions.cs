using System.Security.Claims;

namespace IntegrationGateway.Api.Modules.MancoReporting.Auth;

public static class MancoClaimsPrincipalExtensions
{
    private const string ObjectIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public static string GetAzureAdObjectId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var azureAdObjectId = user.FindFirst("oid")?.Value
            ?? user.FindFirst(ObjectIdentifierClaimType)?.Value;

        if (string.IsNullOrWhiteSpace(azureAdObjectId))
        {
            throw new UnauthorizedAccessException(
                $"Authenticated user token does not contain an Azure AD object id claim. Expected 'oid' or '{ObjectIdentifierClaimType}'.");
        }

        return azureAdObjectId;
    }
}
