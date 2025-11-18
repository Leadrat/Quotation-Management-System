using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class ResendQuotationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IQuotationSendWorkflow _workflow;
        private readonly ILogger<ResendQuotationCommandHandler> _logger;

        public ResendQuotationCommandHandler(
            IAppDbContext db,
            IQuotationSendWorkflow workflow,
            ILogger<ResendQuotationCommandHandler> logger)
        {
            _db = db;
            _workflow = workflow;
            _logger = logger;
        }

        public async Task<QuotationAccessLinkDto> Handle(ResendQuotationCommand command)
        {
            if (command.Request == null)
            {
                throw new ArgumentNullException(nameof(command.Request));
            }

            var quotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.QuotationId == command.QuotationId);

            if (quotation == null)
            {
                throw new QuotationNotFoundException(command.QuotationId);
            }

            await EnsureUserCanSend(command.RequestedByUserId, quotation);
            ValidateQuotationCanBeResent(quotation);

            _logger.LogInformation("Resending quotation {QuotationId} to {RecipientEmail}", quotation.QuotationId, command.Request.RecipientEmail);

            return await _workflow.ExecuteAsync(quotation, command.Request, command.RequestedByUserId, isResend: true);
        }

        private async Task EnsureUserCanSend(Guid userId, Quotation quotation)
        {
            if (quotation.CreatedByUserId == userId)
            {
                return;
            }

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user?.Role == null || !string.Equals(user.Role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("You do not have permission to resend this quotation.");
            }
        }

        private static void ValidateQuotationCanBeResent(Quotation quotation)
        {
            if (quotation.LineItems == null || !quotation.LineItems.Any())
            {
                throw new InvalidOperationException("Quotation must have at least one line item before sending.");
            }

            if (quotation.Status is QuotationStatus.Cancelled or QuotationStatus.Expired or QuotationStatus.Rejected)
            {
                throw new InvalidQuotationStatusException("Cannot resend a quotation in its current status.");
            }
        }
    }
}

 