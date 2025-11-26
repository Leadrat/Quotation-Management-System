namespace CRM.Application.Notifications.Exceptions;

public class UnauthorizedNotificationAccessException : Exception
{
    public UnauthorizedNotificationAccessException(Guid notificationId, Guid userId)
        : base($"User {userId} is not authorized to access notification {notificationId}.")
    {
    }

    public UnauthorizedNotificationAccessException(string message)
        : base(message)
    {
    }

    public UnauthorizedNotificationAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
