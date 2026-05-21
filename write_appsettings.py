import os
import json

config = {
    "ConnectionStrings": {
        "DefaultConnection": os.environ["DB_CONNECTION_STRING"]
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "Graph": {
        "TenantId": os.environ.get("GRAPH_TENANT_ID", ""),
        "ClientId": os.environ.get("GRAPH_CLIENT_ID", ""),
        "ClientSecret": os.environ.get("GRAPH_CLIENT_SECRET", ""),
        "SenderEmail": os.environ.get("SENDER_EMAIL", "")
    },
    "SharePoint": {
        "SiteUrl": os.environ.get("SHAREPOINT_SITE_URL", ""),
        "ListId": os.environ.get("SHAREPOINT_LIST_ID", "")
    },
    "App": {
        "BaseUrl": os.environ.get("APP_BASE_URL", "http://10.200.0.76:8085")
    }
}

with open("publish/appsettings.json", "w") as f:
    json.dump(config, f, indent=2)

print("appsettings.json written successfully")