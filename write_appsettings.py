import os
import json
import sys
from pathlib import Path


def require_env(name: str) -> str:
    value = os.environ.get(name, "").strip()

    if not value:
        print(f"ERROR: Required CI/CD variable is missing or empty: {name}")
        sys.exit(1)

    return value


def optional_env(name: str, default: str = "") -> str:
    return os.environ.get(name, default).strip()


publish_dir = Path("publish")
config_path = publish_dir / "appsettings.json"

if not publish_dir.exists():
    print("ERROR: publish directory does not exist. Run dotnet publish before write_appsettings.py.")
    sys.exit(1)


db_connection_string = require_env("DB_CONNECTION_STRING")
graph_tenant_id = require_env("GRAPH_TENANT_ID")
graph_client_id = require_env("GRAPH_CLIENT_ID")
graph_client_secret = require_env("GRAPH_CLIENT_SECRET")

manco_reporting_connection = optional_env(
    "MANCO_REPORTING_CONNECTION",
    db_connection_string
)

allowed_origins_raw = optional_env("ALLOWED_ORIGINS")

config = {
    "ConnectionStrings": {
        "DefaultConnection": db_connection_string,
        "MancoReportingConnection": manco_reporting_connection
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "Graph": {
        "TenantId": graph_tenant_id,
        "ClientId": graph_client_id,
        "ClientSecret": graph_client_secret,
        "SenderEmail": optional_env("SENDER_EMAIL")
    },
    "SharePoint": {
        "SiteUrl": optional_env("SHAREPOINT_SITE_URL"),
        "ListId": optional_env("SHAREPOINT_LIST_ID"),
        "SiteId": optional_env("SHAREPOINT_SITE_ID"),
        "DriveId": optional_env("SHAREPOINT_DRIVE_ID")
    },
    "App": {
        "BaseUrl": optional_env("APP_BASE_URL", "http://10.200.0.76:8085"),
        "AllowedOrigins": [
            origin.strip()
            for origin in allowed_origins_raw.split(",")
            if origin.strip()
        ]
    }
}

with config_path.open("w", encoding="utf-8") as f:
    json.dump(config, f, indent=2)

print("appsettings.json written successfully")
print("DefaultConnection set:", bool(config["ConnectionStrings"]["DefaultConnection"]))
print("MancoReportingConnection set:", bool(config["ConnectionStrings"]["MancoReportingConnection"]))
print("Graph TenantId set:", bool(config["Graph"]["TenantId"]))
print("Graph ClientId set:", bool(config["Graph"]["ClientId"]))
print("Graph ClientSecret set:", bool(config["Graph"]["ClientSecret"]))
print("SharePoint SiteUrl set:", bool(config["SharePoint"]["SiteUrl"]))
print("App BaseUrl set:", bool(config["App"]["BaseUrl"]))
print("AllowedOrigins count:", len(config["App"]["AllowedOrigins"]))