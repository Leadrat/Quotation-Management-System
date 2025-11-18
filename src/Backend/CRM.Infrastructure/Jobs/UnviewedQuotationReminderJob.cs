using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Quotations.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Jobs
{
    public class UnviewedQuotationReminderJob : CronBackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnviewedQuotationReminderJob> _logger;

        public UnviewedQuotationReminderJob(
            IServiceProvider serviceProvider,
            ILogger<UnviewedQuotationReminderJob> logger)
            : base("0 0 9 * * ?", null, logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<QuotationReminderService>();
            var count = await reminderService.SendUnviewedRemindersAsync(DateTimeOffset.UtcNow, stoppingToken);

            if (count == 0)
            {
                _logger.LogDebug("UnviewedQuotationReminderJob found no quotations requiring reminders");
            }
        }
    }
}


