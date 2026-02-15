using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ContosoUniversity.Models;
using Newtonsoft.Json;

namespace ContosoUniversity.Services
{
    public class NotificationService : IDisposable
    {
        private readonly string _queuePath;
        private readonly ILogger<NotificationService> _logger;

#if WINDOWS
        private System.Messaging.MessageQueue _queue;
        private readonly bool _messagingAvailable;
#endif

        public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _logger = logger;
            
            // Get queue path from configuration or use default
            _queuePath = configuration["NotificationSettings:QueuePath"] ?? @".\Private$\ContosoUniversityNotifications";

#if WINDOWS
            _messagingAvailable = false;
            
            try
            {
                // Ensure the queue exists
                if (!System.Messaging.MessageQueue.Exists(_queuePath))
                {
                    _queue = System.Messaging.MessageQueue.Create(_queuePath);
                    _queue.SetPermissions("Everyone", System.Messaging.MessageQueueAccessRights.FullControl);
                }
                else
                {
                    _queue = new System.Messaging.MessageQueue(_queuePath);
                }
                
                // Configure queue formatter
                _queue.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(string) });
                _messagingAvailable = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize MSMQ notification queue. Notification functionality may not work. Error: {Message}", ex.Message);
            }
#else
            _logger.LogInformation("MSMQ messaging is not available on this platform. Notifications will be logged only.");
#endif
        }

        public void SendNotification(string entityType, string entityId, EntityOperation operation, string userName = null)
        {
            SendNotification(entityType, entityId, null, operation, userName);
        }

        public void SendNotification(string entityType, string entityId, string entityDisplayName, EntityOperation operation, string userName = null)
        {
            var message = GenerateMessage(entityType, entityId, entityDisplayName, operation);
            
            // Always log the notification
            _logger.LogInformation("Notification: {Message} by {User}", message, userName ?? "System");

#if WINDOWS
            if (!_messagingAvailable || _queue == null)
            {
                return;
            }

            try
            {
                var notification = new Notification
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = operation.ToString(),
                    Message = message,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userName ?? "System",
                    IsRead = false
                };

                var jsonMessage = JsonConvert.SerializeObject(notification);
                var queueMessage = new System.Messaging.Message(jsonMessage)
                {
                    Label = $"{entityType} {operation}",
                    Priority = System.Messaging.MessagePriority.Normal
                };

                _queue.Send(queueMessage);
            }
            catch (Exception ex)
            {
                // Log error but don't break the main operation
                _logger.LogError(ex, "Failed to send notification to queue: {Message}", ex.Message);
            }
#endif
        }

        public Notification ReceiveNotification()
        {
#if WINDOWS
            if (!_messagingAvailable || _queue == null)
            {
                return null;
            }

            try
            {
                var message = _queue.Receive(TimeSpan.FromSeconds(1));
                var jsonContent = message.Body.ToString();
                return JsonConvert.DeserializeObject<Notification>(jsonContent);
            }
            catch (System.Messaging.MessageQueueException ex) when (ex.MessageQueueErrorCode == System.Messaging.MessageQueueErrorCode.IOTimeout)
            {
                // No messages available
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to receive notification: {Message}", ex.Message);
                return null;
            }
#else
            return null;
#endif
        }

        public void MarkAsRead(int notificationId)
        {
            // In a real implementation, you might want to store notifications in database as well
            // for persistence and tracking read status
        }

        private string GenerateMessage(string entityType, string entityId, string entityDisplayName, EntityOperation operation)
        {
            var displayText = !string.IsNullOrWhiteSpace(entityDisplayName) 
                ? $"{entityType} '{entityDisplayName}'" 
                : $"{entityType} (ID: {entityId})";

            switch (operation)
            {
                case EntityOperation.CREATE:
                    return $"New {displayText} has been created";
                case EntityOperation.UPDATE:
                    return $"{displayText} has been updated";
                case EntityOperation.DELETE:
                    return $"{displayText} has been deleted";
                default:
                    return $"{displayText} operation: {operation}";
            }
        }

        public void Dispose()
        {
#if WINDOWS
            _queue?.Dispose();
#endif
        }
    }
}
