# Azure Deployment Guide

This document provides instructions for deploying ContosoUniversity to Azure.

## Prerequisites

- Azure subscription
- Azure CLI or Azure Portal access
- .NET 8.0 SDK

## Azure Resources Required

### 1. Azure App Service
- **SKU**: B1 (Basic) or higher
- **Runtime**: .NET 8.0
- **OS**: Linux or Windows

### 2. Azure SQL Database
- **SKU**: Basic or Standard
- **Connection**: Update connection string in App Service configuration

### 3. Application Insights (Optional but Recommended)
- For monitoring and diagnostics

## Deployment Steps

### Option 1: Deploy via Azure CLI

```bash
# Login to Azure
az login

# Create resource group
az group create --name ContosoUniversity-rg --location eastus

# Create App Service plan
az appservice plan create --name ContosoUniversity-plan --resource-group ContosoUniversity-rg --sku B1 --is-linux

# Create web app
az webapp create --name ContosoUniversity-app --resource-group ContosoUniversity-rg --plan ContosoUniversity-plan --runtime "DOTNET|8.0"

# Create Azure SQL Database
az sql server create --name contosouniversity-sqlserver --resource-group ContosoUniversity-rg --location eastus --admin-user sqladmin --admin-password <YourPassword>

az sql db create --resource-group ContosoUniversity-rg --server contosouniversity-sqlserver --name ContosoUniversityDB --service-objective Basic

# Configure connection string
az webapp config connection-string set --name ContosoUniversity-app --resource-group ContosoUniversity-rg --settings DefaultConnection="Server=tcp:contosouniversity-sqlserver.database.windows.net,1433;Database=ContosoUniversityDB;User ID=sqladmin;Password=<YourPassword>;Encrypt=True;TrustServerCertificate=False;" --connection-string-type SQLAzure

# Deploy application
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
cd ..
az webapp deployment source config-zip --resource-group ContosoUniversity-rg --name ContosoUniversity-app --src app.zip
```

### Option 2: Deploy via Visual Studio

1. Right-click on the project → Publish
2. Select Azure → Azure App Service (Linux or Windows)
3. Create new or select existing App Service
4. Configure settings:
   - Connection strings
   - Application settings
5. Publish

### Option 3: Deploy via GitHub Actions

See `.github/workflows/azure-deploy.yml` (to be created)

## Configuration

### Application Settings

Set the following in Azure App Service → Configuration → Application settings:

- `ASPNETCORE_ENVIRONMENT`: Production
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: <Your App Insights Connection String>

### Connection Strings

- `DefaultConnection`: Your Azure SQL Database connection string

## Post-Deployment

### 1. Database Migration

Run EF Core migrations against Azure SQL:

```bash
dotnet ef database update --connection "Server=tcp:contosouniversity-sqlserver.database.windows.net,1433;Database=ContosoUniversityDB;User ID=sqladmin;Password=<YourPassword>;Encrypt=True;TrustServerCertificate=False;"
```

### 2. Verify Deployment

- Navigate to `https://<your-app-name>.azurewebsites.net`
- Check Application Insights for telemetry
- Test all CRUD operations

## Notes on MSMQ

The NotificationService currently uses MSMQ, which is not available in Azure App Service. Consider these alternatives for production:

1. **Azure Service Bus**: Recommended for cloud-native messaging
2. **Azure Queue Storage**: Simple queue storage
3. **Azure SignalR**: For real-time notifications
4. **Database-backed queue**: Store notifications in database

## Monitoring

- Use Application Insights for:
  - Performance monitoring
  - Exception tracking
  - Usage analytics
  - Custom telemetry

## Security Considerations

1. Always use HTTPS (enforced by default)
2. Store secrets in Azure Key Vault
3. Use Managed Identity for database access
4. Enable Azure DDoS Protection
5. Configure CORS appropriately
6. Set up Web Application Firewall (WAF)

## Scaling

- Enable auto-scaling rules in App Service plan
- Consider Azure SQL elastic pools for database
- Use Azure CDN for static content
- Enable caching where appropriate

## Troubleshooting

1. Check Application Insights logs
2. Enable detailed error messages (Development only)
3. Review App Service logs in Azure Portal
4. Use Kudu console for debugging (`https://<app-name>.scm.azurewebsites.net`)

## Cost Optimization

- Use appropriate service tiers
- Enable auto-shutdown for non-production environments
- Monitor resource usage with Azure Cost Management
- Consider reserved instances for production

---

For more information, see:
- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/sql-database/)
- [Application Insights Documentation](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)
