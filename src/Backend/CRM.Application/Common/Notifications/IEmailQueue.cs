using System.Threading.Tasks;

namespace CRM.Application.Common.Notifications
{
    public record EmailMessage(string To, string Subject, string HtmlBody);

    public interface IEmailQueue
    {
        Task EnqueueAsync(EmailMessage message);
    }
}
