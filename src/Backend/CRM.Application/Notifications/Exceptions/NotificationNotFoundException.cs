namespace CRM.Application.Notifications.Exceptions;

public class NotificationNotFoundException : Exception
{
    public NotificationNotFoundException(Guid notificationId)
        : base($"Notification with ID {notificationId} was not found.")
    {
    }

    public NotificationNotFoundException(string message)
        : base(message)
    {
    }

    public NotificationNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
