using System.Threading.Tasks;
using CRM.Domain.Entities;

namespace CRM.Application.Notifications.Services
{
    public interface IEmailNotificationService
    {
        Task SendEmailNotificationAsync(Notification notification, User recipientUser);
        Task RetryFailedEmailsAsync();
        Task LogEmailDeliveryAsync(EmailNotificationLog log);
    }
}

