using System;
using Microsoft.Extensions.Logging;

namespace ContosoUniversity.Services
{
    /// <summary>
    /// Logging service that provides structured logging capabilities using Microsoft.Extensions.Logging
    /// This replaces the legacy Trace.TraceError and Debug.WriteLine calls
    /// </summary>
    public class LoggingService
    {
        private static ILoggerFactory _loggerFactory;
        private static bool _isShuttingDown = false;
        private static readonly object _lock = new object();

        public static void Initialize()
        {
            lock (_lock)
            {
                if (_loggerFactory == null && !_isShuttingDown)
                {
                    _loggerFactory = LoggerFactory.Create(builder =>
                    {
                        // Configure logging providers
                        builder.AddConsole();
                        builder.AddDebug();
                        builder.AddEventLog();
                        
                        // Set minimum log level from configuration
                        var minLogLevel = System.Configuration.ConfigurationManager.AppSettings["MinLogLevel"];
                        LogLevel logLevel = LogLevel.Information;
                        
                        if (!string.IsNullOrEmpty(minLogLevel) && Enum.TryParse(minLogLevel, out LogLevel parsed))
                        {
                            logLevel = parsed;
                        }
                        
                        builder.SetMinimumLevel(logLevel);
                    });
                }
            }
        }

        public static void Shutdown()
        {
            lock (_lock)
            {
                _isShuttingDown = true;
                if (_loggerFactory != null)
                {
                    _loggerFactory.Dispose();
                    _loggerFactory = null;
                }
            }
        }

        public static ILogger<T> CreateLogger<T>()
        {
            lock (_lock)
            {
                if (_isShuttingDown)
                {
                    throw new InvalidOperationException("Cannot create logger after shutdown");
                }
                
                if (_loggerFactory == null)
                {
                    Initialize();
                }
                
                return _loggerFactory.CreateLogger<T>();
            }
        }

        public static ILogger CreateLogger(string categoryName)
        {
            lock (_lock)
            {
                if (_isShuttingDown)
                {
                    throw new InvalidOperationException("Cannot create logger after shutdown");
                }
                
                if (_loggerFactory == null)
                {
                    Initialize();
                }
                
                return _loggerFactory.CreateLogger(categoryName);
            }
        }
    }
}
