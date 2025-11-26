using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using CRM.Domain.Entities;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyDetails.Services;
using CRM.Shared.Config;

namespace CRM.Application.Quotations.Services
{
    public class QuotationEmailService : IQuotationEmailService
    {
        private readonly IFluentEmail? _fluentEmail;
        private readonly IHttpClientFactory? _httpClientFactory;
        private readonly ILogger<QuotationEmailService> _logger;
        private readonly QuotationManagementSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly string _emailProvider;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string? _resendApiKey;
        private readonly ICompanyDetailsService _companyDetailsService;

        public QuotationEmailService(
            IFluentEmail? fluentEmail,
            IHttpClientFactory? httpClientFactory,
            ILogger<QuotationEmailService> logger,
            IOptions<QuotationManagementSettings> settings,
            IConfiguration configuration,
            ICompanyDetailsService companyDetailsService)
        {
            _fluentEmail = fluentEmail;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settings = settings.Value;
            _configuration = configuration;
            _emailProvider = _configuration.GetValue<string>("Email:Provider") ?? "InMemory";
            _fromEmail = _configuration.GetValue<string>("Email:From") ?? "no-reply@crm.com";
            _fromName = _configuration.GetValue<string>("Email:FromName") ?? "CRM System";
            _resendApiKey = _configuration.GetValue<string>("Email:Resend:ApiKey");
            _companyDetailsService = companyDetailsService;
        }

        public async Task SendQuotationEmailAsync(
            Quotation quotation,
            string recipientEmail,
            string accessLink,
            List<string>? ccEmails = null,
            List<string>? bccEmails = null,
            string? customMessage = null)
        {
            if (quotation == null)
            {
                throw new ArgumentNullException(nameof(quotation));
            }
            
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(recipientEmail));
            }
            
            try
            {
                // Get company details (from snapshot or service)
                CompanyDetailsDto? companyDetails = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(quotation.CompanyDetailsSnapshot))
                    {
                        companyDetails = JsonSerializer.Deserialize<CompanyDetailsDto>(quotation.CompanyDetailsSnapshot);
                    }
                    else
                    {
                        companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load company details for quotation {QuotationId}", quotation.QuotationId);
                }

                var viewUrl = ResolveAccessLink(quotation, accessLink);
                var emailBody = GenerateEmailBody(quotation, viewUrl, customMessage, companyDetails);
                var companyName = companyDetails?.CompanyName ?? "Your Company";
                var subject = $"Quotation {quotation.QuotationNumber ?? "N/A"} from {companyName}";

                // Use Resend API if configured
                if (_emailProvider.Equals("Resend", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_resendApiKey) && _httpClientFactory != null)
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_resendApiKey}");
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                    var requestBody = new Dictionary<string, object>
                    {
                        { "from", $"{_fromName} <{_fromEmail}>" },
                        { "to", new[] { recipientEmail } },
                        { "subject", subject },
                        { "html", emailBody }
                    };

                    if (ccEmails != null && ccEmails.Any())
                    {
                        requestBody["cc"] = ccEmails.ToArray();
                    }

                    if (bccEmails != null && bccEmails.Any())
                    {
                        requestBody["bcc"] = bccEmails.ToArray();
                    }

                    try
                    {
                        var jsonContent = JsonSerializer.Serialize(requestBody);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync("https://api.resend.com/emails", content);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                            var messageId = result.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
                            
                            _logger.LogInformation("Quotation email sent successfully via Resend to {RecipientEmail} for quotation {QuotationId}. Message ID: {MessageId}",
                                recipientEmail, quotation.QuotationId, messageId ?? "unknown");
                        }
                        else
                        {
                            _logger.LogError("Failed to send quotation email via Resend to {RecipientEmail}. Status: {Status}, Response: {Response}",
                                recipientEmail, response.StatusCode, responseContent);
                            throw new InvalidOperationException($"Failed to send email via Resend: {response.StatusCode} - {responseContent}");
                        }
                    }
                    catch (Exception resendEx)
                    {
                        _logger.LogError(resendEx, "Resend API error sending email to {RecipientEmail}", recipientEmail);
                        throw new InvalidOperationException($"Failed to send email via Resend: {resendEx.Message}", resendEx);
                    }
                }
                else if (_fluentEmail != null)
                {
                    // Fallback to FluentEmail
                    var email = _fluentEmail
                        .To(recipientEmail)
                        .Subject(subject)
                        .Body(emailBody);

                    if (ccEmails != null && ccEmails.Any())
                    {
                        foreach (var cc in ccEmails)
                        {
                            email.CC(cc);
                        }
                    }

                    if (bccEmails != null && bccEmails.Any())
                    {
                        foreach (var bcc in bccEmails)
                        {
                            email.BCC(bcc);
                        }
                    }

                    var response = await email.SendAsync();

                    if (response == null)
                    {
                        _logger.LogWarning("Email send response is null for quotation {QuotationId}. Email may not be configured properly.", quotation.QuotationId);
                        throw new InvalidOperationException("Email service returned null response. Please check email configuration.");
                    }

                    if (response.Successful)
                    {
                        _logger.LogInformation("Quotation email sent successfully to {RecipientEmail} for quotation {QuotationId}",
                            recipientEmail, quotation.QuotationId);
                    }
                    else
                    {
                        var errorMessages = response.ErrorMessages != null && response.ErrorMessages.Any()
                            ? string.Join(", ", response.ErrorMessages)
                            : "Unknown error";
                        _logger.LogError("Failed to send quotation email to {RecipientEmail}: {Error}",
                            recipientEmail, errorMessages);
                        throw new InvalidOperationException($"Failed to send email: {errorMessages}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("No email service configured. Please configure Resend or SMTP in appsettings.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending quotation email to {RecipientEmail}", recipientEmail);
                throw;
            }
        }

        public async Task SendQuotationAcceptedNotificationAsync(
            Quotation quotation,
            QuotationResponse response,
            string salesRepEmail)
        {
            try
            {
                var email = _fluentEmail
                    .To(salesRepEmail)
                    .Subject($"Good news! {response.ClientName ?? response.ClientEmail} has accepted quotation {quotation.QuotationNumber}")
                    .Body(GenerateAcceptedNotificationBody(quotation, response));

                var emailResponse = await email.SendAsync();

                if (!emailResponse.Successful)
                {
                    _logger.LogError("Failed to send acceptance notification: {Error}",
                        string.Join(", ", emailResponse.ErrorMessages));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending acceptance notification");
            }
        }

        public async Task SendQuotationRejectedNotificationAsync(
            Quotation quotation,
            QuotationResponse response,
            string salesRepEmail)
        {
            try
            {
                var email = _fluentEmail
                    .To(salesRepEmail)
                    .Subject($"Action needed: {response.ClientName ?? response.ClientEmail} has rejected quotation {quotation.QuotationNumber}")
                    .Body(GenerateRejectedNotificationBody(quotation, response));

                var emailResponse = await email.SendAsync();

                if (!emailResponse.Successful)
                {
                    _logger.LogError("Failed to send rejection notification: {Error}",
                        string.Join(", ", emailResponse.ErrorMessages));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending rejection notification");
            }
        }

        private string GenerateCompanyFooter(CompanyDetailsDto? companyDetails)
        {
            if (companyDetails == null)
            {
                return "<p>Your Company Name</p>";
            }

            var footer = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(companyDetails.CompanyName))
            {
                footer.AppendLine($"<p><strong>{companyDetails.CompanyName}</strong></p>");
            }

            // Address
            if (!string.IsNullOrWhiteSpace(companyDetails.CompanyAddress))
            {
                footer.AppendLine($"<p>{companyDetails.CompanyAddress}</p>");
            }
            var addressParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(companyDetails.City)) addressParts.Add(companyDetails.City);
            if (!string.IsNullOrWhiteSpace(companyDetails.State)) addressParts.Add(companyDetails.State);
            if (!string.IsNullOrWhiteSpace(companyDetails.PostalCode)) addressParts.Add(companyDetails.PostalCode);
            if (!string.IsNullOrWhiteSpace(companyDetails.Country)) addressParts.Add(companyDetails.Country);
            if (addressParts.Any())
            {
                footer.AppendLine($"<p>{string.Join(", ", addressParts)}</p>");
            }

            // Contact Info
            if (!string.IsNullOrWhiteSpace(companyDetails.ContactEmail))
            {
                footer.AppendLine($"<p>Email: {companyDetails.ContactEmail}</p>");
            }
            if (!string.IsNullOrWhiteSpace(companyDetails.ContactPhone))
            {
                footer.AppendLine($"<p>Phone: {companyDetails.ContactPhone}</p>");
            }
            if (!string.IsNullOrWhiteSpace(companyDetails.Website))
            {
                footer.AppendLine($"<p>Website: {companyDetails.Website}</p>");
            }

            // Dynamic Country-Specific Identifiers
            if (companyDetails.IdentifierFields != null && companyDetails.IdentifierFields.Any(f => !string.IsNullOrWhiteSpace(f.Value)))
            {
                footer.AppendLine("<p><strong>Company Identifiers:</strong></p>");
                footer.AppendLine("<ul>");
                foreach (var identifier in companyDetails.IdentifierFields.Where(f => !string.IsNullOrWhiteSpace(f.Value)).OrderBy(f => f.DisplayOrder))
                {
                    footer.AppendLine($"<li><strong>{identifier.DisplayName}:</strong> {identifier.Value}</li>");
                }
                footer.AppendLine("</ul>");
            }
            else
            {
                // Fallback to legacy tax information
                var taxInfo = new List<string>();
                if (!string.IsNullOrWhiteSpace(companyDetails.PanNumber)) taxInfo.Add($"PAN: {companyDetails.PanNumber}");
                if (!string.IsNullOrWhiteSpace(companyDetails.TanNumber)) taxInfo.Add($"TAN: {companyDetails.TanNumber}");
                if (!string.IsNullOrWhiteSpace(companyDetails.GstNumber)) taxInfo.Add($"GST: {companyDetails.GstNumber}");
                if (taxInfo.Any())
                {
                    footer.AppendLine($"<p>{string.Join(" | ", taxInfo)}</p>");
                }
            }

            // Dynamic Country-Specific Bank Details
            if (companyDetails.BankFields != null && companyDetails.BankFields.Any(f => !string.IsNullOrWhiteSpace(f.Value)))
            {
                footer.AppendLine("<p><strong>Bank Details:</strong></p>");
                footer.AppendLine("<ul>");
                foreach (var bankField in companyDetails.BankFields.Where(f => !string.IsNullOrWhiteSpace(f.Value)).OrderBy(f => f.DisplayOrder))
                {
                    footer.AppendLine($"<li><strong>{bankField.DisplayName}:</strong> {bankField.Value}</li>");
                }
                footer.AppendLine("</ul>");
                
                // Also show legacy bank details if available
                if (companyDetails.BankDetails != null && companyDetails.BankDetails.Any())
                {
                    var bankDetails = companyDetails.BankDetails.FirstOrDefault();
                    if (bankDetails != null)
                    {
                        if (!string.IsNullOrWhiteSpace(bankDetails.BankName))
                        {
                            footer.AppendLine($"<p><strong>Bank:</strong> {bankDetails.BankName}");
                            if (!string.IsNullOrWhiteSpace(bankDetails.BranchName))
                            {
                                footer.AppendLine($", {bankDetails.BranchName}");
                            }
                            footer.AppendLine("</p>");
                        }
                        if (!string.IsNullOrWhiteSpace(bankDetails.AccountNumber))
                        {
                            footer.AppendLine($"<p><strong>Account Number:</strong> {bankDetails.AccountNumber}</p>");
                        }
                    }
                }
            }
            else if (companyDetails.BankDetails != null && companyDetails.BankDetails.Any())
            {
                // Fallback to legacy bank details
                var bankDetails = companyDetails.BankDetails.FirstOrDefault(b => b.Country == "India") 
                    ?? companyDetails.BankDetails.FirstOrDefault();
                if (bankDetails != null)
                {
                    footer.AppendLine("<p><strong>Bank Details:</strong></p>");
                    footer.AppendLine($"<p>{bankDetails.BankName}");
                    if (!string.IsNullOrWhiteSpace(bankDetails.BranchName))
                    {
                        footer.AppendLine($", {bankDetails.BranchName}");
                    }
                    footer.AppendLine("</p>");
                    footer.AppendLine($"<p>Account Number: {bankDetails.AccountNumber}</p>");
                    if (bankDetails.Country == "India" && !string.IsNullOrWhiteSpace(bankDetails.IfscCode))
                    {
                        footer.AppendLine($"<p>IFSC Code: {bankDetails.IfscCode}</p>");
                    }
                    if (bankDetails.Country == "Dubai")
                    {
                        if (!string.IsNullOrWhiteSpace(bankDetails.Iban))
                        {
                            footer.AppendLine($"<p>IBAN: {bankDetails.Iban}</p>");
                        }
                        if (!string.IsNullOrWhiteSpace(bankDetails.SwiftCode))
                        {
                            footer.AppendLine($"<p>SWIFT Code: {bankDetails.SwiftCode}</p>");
                        }
                    }
                }
            }

            // Legal Disclaimer
            if (!string.IsNullOrWhiteSpace(companyDetails.LegalDisclaimer))
            {
                footer.AppendLine($"<p style=\"font-size: 10px; color: #666; margin-top: 20px;\">{companyDetails.LegalDisclaimer}</p>");
            }

            return footer.ToString();
        }

        private string GenerateEmailBody(Quotation quotation, string accessLink, string? customMessage, CompanyDetailsDto? companyDetails)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2d5016; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #2d5016; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .summary {{ background-color: white; padding: 15px; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Quotation {quotation.QuotationNumber}</h1>
        </div>
        <div class=""content"">
            <p>Dear {quotation.Client?.CompanyName ?? "Client"},</p>
            <p>Your quotation is ready for review. Please click the link below to view the complete quotation online.</p>
            {(string.IsNullOrWhiteSpace(customMessage) ? "" : $"<p>{customMessage}</p>")}
            <div class=""summary"">
                <p><strong>Quotation Date:</strong> {quotation.QuotationDate:dd MMM yyyy}</p>
                <p><strong>Valid Until:</strong> {quotation.ValidUntil:dd MMM yyyy}</p>
                <p><strong>Total Amount:</strong> ₹{quotation.TotalAmount:N2}</p>
            </div>
            <a href=""{accessLink}"" class=""button"">View Quotation Online</a>
            <p>Please review and respond within {quotation.ValidUntil:dd MMM yyyy}.</p>
            <p><em>Note: This quotation is available online through the secure link above. No PDF attachment is included for security reasons.</em></p>
        </div>
        <div class=""footer"">
            <p>Thank you for your business!</p>
            {GenerateCompanyFooter(companyDetails)}
        </div>
    </div>
</body>
</html>";
        }

        public async Task SendUnviewedQuotationReminderAsync(
            Quotation quotation,
            string salesRepEmail,
            DateTimeOffset sentAt)
        {
            var email = _fluentEmail
                .To(salesRepEmail)
                .Subject($"Reminder: Quotation {quotation.QuotationNumber} has not been viewed")
                .Body($@"
<!DOCTYPE html>
<html>
<body style=""font-family: Arial, sans-serif;"">
    <p>Hello,</p>
    <p>Your quotation <strong>{quotation.QuotationNumber}</strong> sent on {sentAt:dd MMM yyyy} has not been viewed yet.</p>
    <p>Please consider following up with the client to ensure they received it.</p>
    <p>-- CRM System</p>
</body>
</html>");

            var response = await email.SendAsync();
            if (!response.Successful)
            {
                _logger.LogWarning("Failed to send unviewed reminder for quotation {QuotationId}: {Error}",
                    quotation.QuotationId, string.Join(", ", response.ErrorMessages));
            }
        }

        public async Task SendPendingResponseFollowUpAsync(
            Quotation quotation,
            string salesRepEmail,
            DateTimeOffset firstViewedAt)
        {
            var email = _fluentEmail
                .To(salesRepEmail)
                .Subject($"Follow-up: Awaiting response on quotation {quotation.QuotationNumber}")
                .Body($@"
<!DOCTYPE html>
<html>
<body style=""font-family: Arial, sans-serif;"">
    <p>Hello,</p>
    <p>The client viewed quotation <strong>{quotation.QuotationNumber}</strong> on {firstViewedAt:dd MMM yyyy}, but no response has been recorded.</p>
    <p>This is a gentle reminder to follow up and keep the deal moving.</p>
    <p>-- CRM System</p>
</body>
</html>");

            var response = await email.SendAsync();
            if (!response.Successful)
            {
                _logger.LogWarning("Failed to send pending response follow-up for quotation {QuotationId}: {Error}",
                    quotation.QuotationId, string.Join(", ", response.ErrorMessages));
            }
        }

        private string ResolveAccessLink(Quotation quotation, string? providedAccessLink)
        {
            if (!string.IsNullOrWhiteSpace(providedAccessLink))
            {
                return providedAccessLink;
            }

            var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl)
                ? "https://crm.example.com"
                : _settings.BaseUrl;

            return $"{baseUrl.TrimEnd('/')}/client-portal/quotations/{quotation.QuotationId}";
        }

        public async Task SendSimpleEmailAsync(string recipientEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(recipientEmail));
            }

            try
            {
                // Use Resend API if configured
                if (_emailProvider.Equals("Resend", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_resendApiKey) && _httpClientFactory != null)
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_resendApiKey}");
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                    var requestBody = new Dictionary<string, object>
                    {
                        { "from", $"{_fromName} <{_fromEmail}>" },
                        { "to", new[] { recipientEmail } },
                        { "subject", subject },
                        { "html", htmlBody }
                    };

                    try
                    {
                        var jsonContent = JsonSerializer.Serialize(requestBody);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync("https://api.resend.com/emails", content);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Simple email sent successfully via Resend to {RecipientEmail}", recipientEmail);
                        }
                        else
                        {
                            _logger.LogError("Failed to send simple email via Resend to {RecipientEmail}. Status: {Status}, Response: {Response}",
                                recipientEmail, response.StatusCode, responseContent);
                            throw new InvalidOperationException($"Failed to send email via Resend: {response.StatusCode} - {responseContent}");
                        }
                    }
                    catch (Exception resendEx)
                    {
                        _logger.LogError(resendEx, "Resend API error sending simple email to {RecipientEmail}", recipientEmail);
                        throw new InvalidOperationException($"Failed to send email via Resend: {resendEx.Message}", resendEx);
                    }
                }
                else if (_fluentEmail != null)
                {
                    // Fallback to FluentEmail
                    var email = _fluentEmail
                        .To(recipientEmail)
                        .Subject(subject)
                        .Body(htmlBody, true);

                    var emailResponse = await email.SendAsync();

                    if (emailResponse != null && emailResponse.Successful)
                    {
                        _logger.LogInformation("Simple email sent successfully to {RecipientEmail}", recipientEmail);
                    }
                    else
                    {
                        var errorMessages = emailResponse?.ErrorMessages != null && emailResponse.ErrorMessages.Any()
                            ? string.Join(", ", emailResponse.ErrorMessages)
                            : "Unknown error";
                        _logger.LogError("Failed to send simple email to {RecipientEmail}: {Error}", recipientEmail, errorMessages);
                        throw new InvalidOperationException($"Failed to send email: {errorMessages}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("No email service configured. Please configure Resend or SMTP in appsettings.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending simple email to {RecipientEmail}", recipientEmail);
                throw;
            }
        }

        private string GenerateAcceptedNotificationBody(Quotation quotation, QuotationResponse response)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .success {{ background-color: #d4edda; color: #155724; padding: 15px; border-radius: 4px; margin: 20px 0; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""success"">
            <h2>✓ ACCEPTED</h2>
        </div>
        <div class=""content"">
            <p><strong>{response.ClientName ?? response.ClientEmail}</strong> has accepted quotation <strong>{quotation.QuotationNumber}</strong>.</p>
            {(string.IsNullOrWhiteSpace(response.ResponseMessage) ? "" : $"<p><strong>Client Message:</strong> {response.ResponseMessage}</p>")}
            <p><strong>Total Amount:</strong> ₹{quotation.TotalAmount:N2}</p>
            <p><strong>Response Date:</strong> {response.ResponseDate:dd MMM yyyy HH:mm}</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateRejectedNotificationBody(Quotation quotation, QuotationResponse response)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .error {{ background-color: #f8d7da; color: #721c24; padding: 15px; border-radius: 4px; margin: 20px 0; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""error"">
            <h2>✗ REJECTED</h2>
        </div>
        <div class=""content"">
            <p><strong>{response.ClientName ?? response.ClientEmail}</strong> has rejected quotation <strong>{quotation.QuotationNumber}</strong>.</p>
            {(string.IsNullOrWhiteSpace(response.ResponseMessage) ? "" : $"<p><strong>Client Message:</strong> {response.ResponseMessage}</p>")}
            <p><strong>Total Amount:</strong> ₹{quotation.TotalAmount:N2}</p>
            <p><strong>Response Date:</strong> {response.ResponseDate:dd MMM yyyy HH:mm}</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}

