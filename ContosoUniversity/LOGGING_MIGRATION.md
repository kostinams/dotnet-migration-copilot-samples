# Logging Migration Documentation

## Overview
This application has been migrated from legacy `System.Diagnostics.Trace` and `System.Diagnostics.Debug` logging to structured logging using `Microsoft.Extensions.Logging`.

## Changes Made

### 1. Dependencies Added
- `Microsoft.Extensions.Logging.Console` (version 3.1.32)
- `Microsoft.Extensions.Logging.Debug` (version 3.1.32)
- `Microsoft.Extensions.Logging.EventLog` (version 3.1.32)

### 2. New Infrastructure
- **LoggingService.cs**: Centralized logging service that initializes and provides `ILogger` instances
  - Configures Console, Debug, and EventLog logging providers
  - Reads minimum log level from Web.config (default: Information)
  - Thread-safe singleton pattern for logger factory

### 3. Configuration
Added to `Web.config`:
```xml
<add key="MinLogLevel" value="Information"/>
```

Supported values: Trace, Debug, Information, Warning, Error, Critical

### 4. Code Changes

#### Controllers Updated:
- **StudentsController**: Replaced `Trace.TraceError()` with `ILogger.LogError()`
  - Structured logging with named parameters
  - Exception details automatically captured
  
- **BaseController**: Replaced `Debug.WriteLine()` with `ILogger.LogWarning()`
  - Added protected `_logger` field for derived controllers
  
- **NotificationsController**: Replaced `Debug.WriteLine()` with `ILogger.LogError()`
  - Added constructor to initialize logger
  
- **CoursesController**: Replaced `Debug.WriteLine()` with `ILogger.LogWarning()`
  - File deletion errors now properly logged

#### Services Updated:
- **NotificationService**: Replaced `Debug.WriteLine()` with `ILogger.LogWarning()`
  - Queue operation failures now properly logged

### 5. Logging Patterns Used

#### Old Pattern (Trace/Debug):
```csharp
Trace.TraceError($"Error creating student: {ex.Message} | Student: {student?.FirstMidName} | Stack: {ex.StackTrace}");
Debug.WriteLine($"Failed to send notification: {ex.Message}");
```

#### New Pattern (ILogger):
```csharp
_logger.LogError(ex, "Error creating student. Student: {FirstName} {LastName}, EnrollmentDate: {EnrollmentDate}", 
    student?.FirstMidName, student?.LastName, student?.EnrollmentDate);
_logger.LogWarning(ex, "Failed to send notification for {EntityType} {EntityId}", entityType, entityId);
```

## Benefits

### 1. Structured Logging
- Named parameters for better querying and filtering
- Consistent format across all log entries
- Automatic exception details capture

### 2. Multiple Outputs
- Console (for development and debugging)
- Debug output (for Visual Studio debugging)
- Windows Event Log (for production monitoring)

### 3. Configurable Log Levels
- Control verbosity via configuration
- Different levels for different environments
- No code changes required

### 4. Future Extensibility
The logging infrastructure is now ready for:
- OpenTelemetry integration
- Application Insights integration
- Serilog or other advanced logging frameworks
- Custom log sinks (files, databases, etc.)

## Next Steps for OpenTelemetry

To add OpenTelemetry, you would:

1. Add NuGet packages:
   ```
   OpenTelemetry.Extensions.Hosting
   OpenTelemetry.Exporter.Console
   OpenTelemetry.Exporter.OpenTelemetryProtocol
   ```

2. Update LoggingService.cs to add OpenTelemetry:
   ```csharp
   builder.AddOpenTelemetry(options =>
   {
       options.AddConsoleExporter();
       options.AddOtlpExporter();
   });
   ```

3. Configure OTLP endpoint in Web.config:
   ```xml
   <add key="OtlpEndpoint" value="http://localhost:4317"/>
   ```

## Testing

To verify logging is working:

1. Set `MinLogLevel` to `Debug` in Web.config
2. Run the application
3. Perform operations (create/edit/delete students)
4. Check:
   - Console output (if running in console mode)
   - Debug output in Visual Studio Output window
   - Windows Event Viewer (Application logs)

## Troubleshooting

### Logs not appearing
- Check `MinLogLevel` setting in Web.config
- Ensure LoggingService.Initialize() is called in Global.asax.cs
- Verify log level matches the severity of the message

### Event Log permissions
- Application must have permissions to write to Event Log
- Run with elevated privileges or configure Event Log permissions

## Maintenance

When adding new logging:
```csharp
// In constructor
private readonly ILogger<YourController> _logger;

public YourController()
{
    _logger = LoggingService.CreateLogger<YourController>();
}

// In methods
_logger.LogInformation("User {UserId} performed action", userId);
_logger.LogWarning("Validation failed for {Entity}", entityName);
_logger.LogError(ex, "Failed to process {Operation}", operationName);
```

## Migration Date
2026-02-19

## References
- [Microsoft.Extensions.Logging Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
- [Structured Logging Best Practices](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging-best-practices)
