# Control Plane Authentication

## Architecture

```text
IncidentOps Angular SPA
Future SPFx web parts
Future Moodle/LMS confidential client
Future Zoho Desk extension
Other internal apps
        |
        | Microsoft Entra ID access token
        v
Integration Gateway API
  - validates authentication
  - resolves calling client application
  - normalizes user/audit context
  - maps roles, scopes, groups, and permissions
  - brokers downstream integrations
        |
        | future on-behalf-of or app-only access
        v
Microsoft Graph, SharePoint, LMS, Zoho, and other services
```

The Integration Gateway API is the protected resource and backend control plane. Frontend applications authenticate with Microsoft Entra ID through MSAL/OIDC and call the Gateway with Bearer access tokens. Server-side integrations use confidential clients and application roles.

## App Registrations

Gateway API app registration:
- Exposes the API scopes.
- Defines application roles for server-to-server and privileged client access.
- Uses `AzureAd:ClientId` as the API app registration client ID.
- Uses `AzureAd:Audience` for the Application ID URI, typically `api://{gateway-api-client-id}`.

IncidentOps Angular SPA client app registration:
- Public SPA client.
- Requests delegated Gateway API scopes.
- Local redirect URI: `https://localhost:4200`.
- Production redirect URI placeholder: `https://incidentops.company.local`.

Future SPFx client:
- Acquires an AAD token for the Gateway API scope.
- Calls the Gateway API instead of directly embedding integration secrets or business permissions in the web part.

Future Moodle/LMS client:
- Treat Moodle as a confidential client/server-side application.
- Prefer client credentials plus Gateway app roles for backend LMS integration.
- Do not expose Moodle client secrets in browser-delivered code.

Future Zoho Desk extension:
- Prefer delegated user tokens where the extension acts for an internal user.
- Use confidential server-side mediation when the extension needs integration secrets.

## Exposed Scopes

Use these delegated scopes on the Gateway API app registration:

```text
api://{gateway-api-client-id}/incidents.read
api://{gateway-api-client-id}/incidents.write
api://{gateway-api-client-id}/attachments.write
api://{gateway-api-client-id}/workflow.transition
api://{gateway-api-client-id}/admin.full
```

## Suggested App Roles

```text
Gateway.Admin
IncidentOps.Admin
IncidentOps.Support
IncidentOps.Operations
IncidentOps.Accounts
IncidentOps.Management
```

## Local Development

`ControlPlane:DevelopmentAuthEnabled` is disabled by default. If set to `true`, it only works when `ASPNETCORE_ENVIRONMENT` is `Development`; production startup throws if it is enabled.

The development principal is:

```json
{
  "userId": "dev-user",
  "displayName": "Development Admin",
  "email": "dev@local",
  "roles": ["Gateway.Admin", "IncidentOps.Admin"]
}
```

MSAL requires a secure browser context. `http://10.200.0.76:8086` is not a secure context and can fail with `crypto_nonexistent`. Use HTTPS for local and production browser clients.

## API Key Compatibility

Existing API key protected routes remain supported when `ControlPlane:AllowApiKeyFallback` is `true`. If a request sends an `Authorization: Bearer ...` header, the API key middleware does not require `X-Api-Key`; JWT authentication handles the request.

## Client Applications Table

The code includes a `ClientApplication` EF entity and `DbSet`. If migrations are not used for this database-first project, create the table with a script equivalent to:

```sql
CREATE TABLE dbo.ClientApplications (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ClientApplications PRIMARY KEY,
    Name nvarchar(150) NOT NULL,
    ClientId nvarchar(100) NOT NULL,
    AppType nvarchar(50) NOT NULL,
    AuthProvider nvarchar(50) NOT NULL,
    AllowedOrigins nvarchar(1000) NULL,
    IsActive bit NOT NULL CONSTRAINT DF_ClientApplications_IsActive DEFAULT (1),
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_ClientApplications_CreatedAt DEFAULT (sysutcdatetime()),
    UpdatedAt datetime2(0) NULL
);

CREATE UNIQUE INDEX UX_ClientApplications_ClientId
ON dbo.ClientApplications (ClientId);
```

If the table is empty or unavailable during early rollout, the Gateway falls back to the token `azp` or `appid` claim.

## Future OBO

On-behalf-of flow can be added later when the Gateway needs to call Microsoft Graph as the signed-in user. Keep Graph access centralized in the Gateway and request only the downstream scopes required by each workflow.
