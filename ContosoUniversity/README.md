# Contoso University - .NET 8.0

This project is an ASP.NET Core MVC application targeting .NET 8.0, migrated from ASP.NET MVC 5 (.NET Framework 4.8).

## Project Overview

### Framework
- **ASP.NET Core MVC 8.0** (.NET 8.0 LTS)
- **Migration Date**: 2025
- **Original Framework**: ASP.NET MVC 5 (.NET Framework 4.8)

### Database Access
- **Entity Framework Core 8.0.11**
- **Database Provider**: SQL Server (Microsoft.EntityFrameworkCore.SqlServer 8.0.11)

### Key Technologies
- ASP.NET Core MVC 8.0
- Entity Framework Core 8.0
- Dependency Injection
- Configuration via appsettings.json
- Logging with ILogger
- Application Insights (ready for Azure)
- Tag Helpers for views
- Async/await patterns

### Project Structure
```
ContosoUniversity/
├── Controllers/            # ASP.NET Core MVC Controllers
├── Data/                   # EF Core context, initializer, and factory
├── Models/                 # Data models and view models
├── Services/               # Business services (NotificationService)
├── Views/                  # Razor views with Tag Helpers
├── wwwroot/                # Static files (CSS, JavaScript, uploads)
│   ├── css/                # Stylesheets
│   ├── js/                 # JavaScript files
│   └── uploads/            # User uploaded files
├── Properties/             # Launch settings
├── Program.cs              # Application entry point and configuration
├── appsettings.json        # Application configuration
├── appsettings.Development.json  # Development configuration
├── appsettings.Production.json   # Production configuration
└── AZURE_DEPLOYMENT.md     # Azure deployment guide
```

## Database Configuration

The application uses SQL Server LocalDB (development) or Azure SQL (production) with connection strings configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True;MultipleActiveResultSets=True"
  }
}
```

For production, configure connection strings via Azure App Service Configuration or environment variables.

## Running the Application

### Prerequisites
- **.NET 8.0 SDK** or later
- **SQL Server LocalDB** (for development) or SQL Server/Azure SQL
- **IDE**: Visual Studio 2022, VS Code, or Rider

### Setup

1. **Install .NET 8.0 SDK**:
   ```bash
   # Verify installation
   dotnet --version
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Update database** (creates and seeds database):
   ```bash
   dotnet ef database update
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```
   
   Or use Visual Studio/VS Code to run with debugging.

5. **Access the application**:
   - Development: `https://localhost:5001` or `http://localhost:5000`
   - The database will be automatically created and seeded on first run

### Development

- **Build**: `dotnet build`
- **Run**: `dotnet run`
- **Watch** (auto-reload): `dotnet watch run`
- **Test**: `dotnet test` (if tests exist)
- **Clean**: `dotnet clean`

## Features

- **Student Management**: Complete CRUD operations with pagination, sorting, and search
- **Course Management**: Manage courses, assignments to departments, and teaching materials
- **Instructor Management**: Handle instructor assignments, office locations, and courses
- **Department Management**: Manage departments, administrators, and budgets
- **Notifications**: Entity change notifications with logging
- **Statistics**: View enrollment statistics by date
- **File Uploads**: Support for teaching material uploads
- **Responsive UI**: Bootstrap 5-based responsive design

## Database Initialization

The application uses Entity Framework Core Code First with automatic database initialization:
- Creates database if it doesn't exist
- Seeds sample data (students, instructors, courses, departments, enrollments)
- Initialization runs automatically on application startup
- Uses migrations for schema management

### EF Core Migrations

```bash
# Create a new migration
dotnet ef migrations add <MigrationName>

# Update database to latest migration
dotnet ef database update

# List migrations
dotnet ef migrations list

# Remove last migration (if not applied)
dotnet ef migrations remove
```

## Azure Deployment

See [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md) for detailed Azure deployment instructions.

### Quick Azure Deploy

```bash
# Build for production
dotnet publish -c Release -o ./publish

# Deploy to Azure (requires Azure CLI)
az webapp deployment source config-zip --resource-group <your-rg> --name <your-app> --src ./publish.zip
```

## Configuration

### Application Settings

Configuration is managed through `appsettings.json` and environment-specific files:

- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Development overrides
- `appsettings.Production.json`: Production overrides

### Key Configuration Sections

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Logging": {
    "LogLevel": { ... }
  },
  "NotificationSettings": {
    "QueuePath": ".\\Private$\\ContosoUniversityNotifications"
  },
  "ApplicationInsights": {
    "ConnectionString": "..."
  }
}
```

## Logging

The application uses ASP.NET Core's built-in logging with support for:
- Console logging (Development)
- Application Insights (Production)
- Configurable log levels via appsettings.json

## Architecture

### Dependency Injection

All services are registered in `Program.cs`:
- `SchoolContext`: Scoped (per-request)
- `NotificationService`: Scoped
- Controllers: Transient (default)

### Middleware Pipeline

1. Exception Handling (Development: Developer Exception Page, Production: Error Handler)
2. HTTPS Redirection
3. Static Files
4. Routing
5. Authorization
6. Endpoint Mapping

## Migration Notes

This application was migrated from .NET Framework 4.8 to .NET 8.0 with the following changes:

### Completed Migration Steps
✅ Converted to SDK-style project  
✅ Updated to .NET 8.0 target framework  
✅ Migrated to ASP.NET Core MVC 8.0  
✅ Updated Entity Framework Core from 3.1 to 8.0  
✅ Replaced Web.config with appsettings.json  
✅ Implemented dependency injection throughout  
✅ Migrated views to Tag Helpers  
✅ Moved static files to wwwroot  
✅ Updated all controllers to ASP.NET Core patterns  
✅ Modernized logging with ILogger  
✅ Added Application Insights support  
✅ Azure deployment ready  

### Breaking Changes from .NET Framework
- MSMQ notifications now use conditional compilation (Windows only)
- File uploads changed from `HttpPostedFileBase` to `IFormFile`
- Configuration access changed from `ConfigurationManager` to `IConfiguration`
- Removed dependency on `System.Web`

## Troubleshooting

### Database Issues
```bash
# Drop and recreate database
dotnet ef database drop
dotnet ef database update
```

### Port Already in Use
Update `Properties/launchSettings.json` to use different ports.

### Build Errors
```bash
dotnet clean
dotnet restore
dotnet build
```

## License

This is a sample application for educational purposes.

## Support

For issues and questions:
- Check [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md) for deployment help
- Review Application Insights for runtime issues
- Check console logs for development errors
