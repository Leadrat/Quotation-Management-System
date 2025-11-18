using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class SubmitQuotationResponseCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IQuotationEmailService _emailService;
        private readonly ILogger<SubmitQuotationResponseCommandHandler> _logger;

        public SubmitQuotationResponseCommandHandler(
            IAppDbContext db,
            IQuotationEmailService emailService,
            ILogger<SubmitQuotationResponseCommandHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<QuotationResponseDto> Handle(SubmitQuotationResponseCommand command)
        {
            if (command.Request == null)
            {
                throw new ArgumentNullException(nameof(command.Request));
            }

            var link = await _db.QuotationAccessLinks
                .Include(x => x.Quotation)
                    .ThenInclude(q => q.Client)
                .Include(x => x.Quotation)
                    .ThenInclude(q => q.CreatedByUser)
                .Include(x => x.Quotation)
                    .ThenInclude(q => q.LineItems)
                .FirstOrDefaultAsync(x => x.AccessToken == command.AccessToken);

            if (link == null)
            {
                throw new QuotationAccessLinkNotFoundException();
            }

            if (!link.IsActive || link.IsExpired())
            {
                throw new InvalidOperationException("Access link is inactive or expired.");
            }

            var quotation = link.Quotation ?? throw new QuotationNotFoundException(link.QuotationId);

            var existingResponse = await _db.QuotationResponses
                .FirstOrDefaultAsync(x => x.QuotationId == quotation.QuotationId);

            if (existingResponse != null)
            {
                throw new InvalidOperationException("Quotation response already submitted.");
            }

            var response = new QuotationResponse
            {
                ResponseId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                ResponseType = command.Request.ResponseType.ToUpperInvariant(),
                ClientEmail = command.Request.ClientEmail ?? link.ClientEmail,
                ClientName = command.Request.ClientName ?? quotation.Client?.CompanyName,
                ResponseMessage = command.Request.ResponseMessage,
                ResponseDate = DateTimeOffset.UtcNow,
                IpAddress = command.IpAddress,
                UserAgent = null
            };

            _db.QuotationResponses.Add(response);

            var previousStatus = quotation.Status;
            var newStatus = DetermineNewStatus(response.ResponseType, quotation.Status);
            quotation.Status = newStatus;
            quotation.UpdatedAt = DateTimeOffset.UtcNow;

            _db.QuotationStatusHistory.Add(new QuotationStatusHistory
            {
                HistoryId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                PreviousStatus = previousStatus.ToString(),
                NewStatus = newStatus.ToString(),
                ChangedAt = DateTimeOffset.UtcNow,
                Reason = $"Client responded: {response.ResponseType}",
                IpAddress = command.IpAddress
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "Database error saving quotation response. Error: {Error}", innerMessage);
                
                // Check for missing table
                if (innerMessage.Contains("42P01") || innerMessage.Contains("does not exist") || innerMessage.Contains("QuotationResponses"))
                {
                    throw new InvalidOperationException("Database table missing. Please contact administrator to run migrations.");
                }
                
                // Check for foreign key violations
                if (innerMessage.Contains("foreign key") || innerMessage.Contains("violates foreign key"))
                {
                    throw new InvalidOperationException("Invalid quotation or related data not found.");
                }
                
                throw new InvalidOperationException($"Failed to save quotation response: {innerMessage}", dbEx);
            }

            await NotifySalesRepAsync(quotation, response);

            var eventPayload = new QuotationResponseReceived
            {
                QuotationId = quotation.QuotationId,
                ResponseId = response.ResponseId,
                ResponseType = response.ResponseType,
                ResponseDate = response.ResponseDate,
                ClientEmail = response.ClientEmail
            };

            _ = eventPayload;

            return new QuotationResponseDto
            {
                ResponseId = response.ResponseId,
                QuotationId = response.QuotationId,
                ResponseType = response.ResponseType,
                ClientEmail = response.ClientEmail,
                ClientName = response.ClientName,
                ResponseMessage = response.ResponseMessage,
                ResponseDate = response.ResponseDate,
                IpAddress = response.IpAddress
            };
        }

        private static QuotationStatus DetermineNewStatus(string responseType, QuotationStatus currentStatus)
        {
            return responseType switch
            {
                "ACCEPTED" => QuotationStatus.Accepted,
                "REJECTED" => QuotationStatus.Rejected,
                _ => QuotationStatus.Viewed
            };
        }

        private async Task NotifySalesRepAsync(Quotation quotation, QuotationResponse response)
        {
            try
            {
                // Notify sales rep
                var salesEmail = quotation.CreatedByUser?.Email;
                if (!string.IsNullOrWhiteSpace(salesEmail))
                {
                    if (response.ResponseType == "ACCEPTED")
                    {
                        await _emailService.SendQuotationAcceptedNotificationAsync(quotation, response, salesEmail);
                    }
                    else if (response.ResponseType == "REJECTED")
                    {
                        await _emailService.SendQuotationRejectedNotificationAsync(quotation, response, salesEmail);
                    }
                    else
                    {
                        await _emailService.SendQuotationRejectedNotificationAsync(quotation, response, salesEmail);
                    }
                }

                // Notify all admins
                var adminUsers = await _db.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role != null && 
                        string.Equals(u.Role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) &&
                        u.IsActive && 
                        u.DeletedAt == null &&
                        !string.IsNullOrWhiteSpace(u.Email))
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    try
                    {
                        if (response.ResponseType == "ACCEPTED")
                        {
                            await _emailService.SendQuotationAcceptedNotificationAsync(quotation, response, admin.Email);
                        }
                        else if (response.ResponseType == "REJECTED")
                        {
                            await _emailService.SendQuotationRejectedNotificationAsync(quotation, response, admin.Email);
                        }
                        else
                        {
                            await _emailService.SendQuotationRejectedNotificationAsync(quotation, response, admin.Email);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification to admin {AdminEmail}", admin.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send quotation response notification.");
            }
        }
    }
}
