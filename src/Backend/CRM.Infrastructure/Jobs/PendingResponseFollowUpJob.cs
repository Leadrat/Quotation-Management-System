using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Quotations.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Jobs
{
    public class PendingResponseFollowUpJob : CronBackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PendingResponseFollowUpJob> _logger;

        public PendingResponseFollowUpJob(
            IServiceProvider serviceProvider,
            ILogger<PendingResponseFollowUpJob> logger)
            : base("0 0 15 * * ?", null, logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<QuotationReminderService>();
            var count = await reminderService.SendPendingResponseFollowUpsAsync(DateTimeOffset.UtcNow, stoppingToken);

            if (count == 0)
            {
                _logger.LogDebug("PendingResponseFollowUpJob found no quotations requiring follow-up");
            }
        }
    }
}


