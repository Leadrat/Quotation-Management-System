using System.Threading.Tasks;
using CRM.Domain.Entities;

namespace CRM.Application.Notifications.Services
{
    public interface IEmailNotificationService
    {
        Task SendEmailNotificationAsync(UserNotification notification, User recipientUser);
        Task RetryFailedEmailsAsync();
        Task LogEmailDeliveryAsync(EmailNotificationLog log);
        
        // New methods for dispatch service
        Task<EmailResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
        Task<EmailResult> SendEmailAsync(List<string> to, string subject, string body, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Result of email sending operation
    /// </summary>
    public class EmailResult
    {
        public bool IsSuccess { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }
    }
}

