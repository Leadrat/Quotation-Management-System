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
    public class SendQuotationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IQuotationSendWorkflow _workflow;
        private readonly ILogger<SendQuotationCommandHandler> _logger;

        public SendQuotationCommandHandler(
            IAppDbContext db,
            IQuotationSendWorkflow workflow,
            ILogger<SendQuotationCommandHandler> logger)
        {
            _db = db;
            _workflow = workflow;
            _logger = logger;
        }

        public async Task<QuotationAccessLinkDto> Handle(SendQuotationCommand command)
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
            ValidateQuotationCanBeSent(quotation);

            _logger.LogInformation("Sending quotation {QuotationId} to {RecipientEmail}", quotation.QuotationId, command.Request.RecipientEmail);

            return await _workflow.ExecuteAsync(quotation, command.Request, command.RequestedByUserId, isResend: false);
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

            if (user?.Role == null)
            {
                throw new UnauthorizedAccessException("You do not have permission to send this quotation.");
            }

            var roleName = user.Role.RoleName;
            var isSalesRep = string.Equals(roleName, "SalesRep", StringComparison.OrdinalIgnoreCase);
            
            // Only SalesRep can send quotations (Admin removed from this functionality)
            if (!isSalesRep)
            {
                throw new UnauthorizedAccessException("Only Sales Representatives can send quotations.");
            }
        }

        private static void ValidateQuotationCanBeSent(Quotation quotation)
        {
            if (quotation.Status != QuotationStatus.Draft)
            {
                throw new InvalidQuotationStatusException("Only draft quotations can be sent.");
            }

            if (quotation.LineItems == null || !quotation.LineItems.Any())
            {
                throw new InvalidOperationException("Quotation must have at least one line item before sending.");
            }
        }
    }
}

 
