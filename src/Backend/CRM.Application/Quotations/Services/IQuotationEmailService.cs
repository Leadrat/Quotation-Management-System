using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.Entities;

namespace CRM.Application.Quotations.Services
{
    public interface IQuotationEmailService
    {
        Task SendQuotationEmailAsync(
            Quotation quotation,
            string recipientEmail,
            byte[] pdfAttachment,
            string accessLink,
            List<string>? ccEmails = null,
            List<string>? bccEmails = null,
            string? customMessage = null);

        Task SendQuotationAcceptedNotificationAsync(
            Quotation quotation,
            QuotationResponse response,
            string salesRepEmail);

        Task SendQuotationRejectedNotificationAsync(
            Quotation quotation,
            QuotationResponse response,
            string salesRepEmail);

        Task SendUnviewedQuotationReminderAsync(
            Quotation quotation,
            string salesRepEmail,
            DateTimeOffset sentAt);

        Task SendPendingResponseFollowUpAsync(
            Quotation quotation,
            string salesRepEmail,
            DateTimeOffset firstViewedAt);

        Task SendSimpleEmailAsync(
            string recipientEmail,
            string subject,
            string htmlBody);
    }
}


