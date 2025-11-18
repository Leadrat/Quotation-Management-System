using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Quotations.Commands;
using CRM.Application.Quotations.Commands.Handlers;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Jobs
{
    public class QuotationExpirationCheckJob : CronBackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QuotationExpirationCheckJob> _logger;

        public QuotationExpirationCheckJob(
            IServiceProvider serviceProvider,
            ILogger<QuotationExpirationCheckJob> logger)
            : base("0 0 0 * * ?", null, logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var handler = scope.ServiceProvider.GetRequiredService<MarkQuotationAsExpiredCommandHandler>();

            var today = DateTime.Today;
            var candidates = await db.Quotations
                .Where(q =>
                    q.ValidUntil < today &&
                    q.Status != QuotationStatus.Accepted &&
                    q.Status != QuotationStatus.Rejected &&
                    q.Status != QuotationStatus.Expired &&
                    q.Status != QuotationStatus.Cancelled)
                .Select(q => q.QuotationId)
                .ToListAsync(stoppingToken);

            var processed = 0;
            foreach (var quotationId in candidates)
            {
                await handler.Handle(new MarkQuotationAsExpiredCommand
                {
                    QuotationId = quotationId,
                    Reason = "Quotation expired automatically"
                });
                processed++;
            }

            if (processed > 0)
            {
                _logger.LogInformation("QuotationExpirationCheckJob marked {Count} quotations as expired", processed);
            }
        }
    }
}


