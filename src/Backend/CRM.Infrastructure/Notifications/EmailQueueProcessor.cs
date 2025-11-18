using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CRM.Application.Common.Notifications;

namespace CRM.Infrastructure.Notifications
{
    public class InMemoryEmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _channel;

        public InMemoryEmailQueue(Channel<EmailMessage> channel)
        {
            _channel = channel;
        }

        public Task EnqueueAsync(EmailMessage message)
        {
            return _channel.Writer.WriteAsync(message).AsTask();
        }
    }

    public class EmailQueueProcessor : BackgroundService
    {
        private readonly Channel<EmailMessage> _channel;
        private readonly ILogger<EmailQueueProcessor> _logger;

        public EmailQueueProcessor(Channel<EmailMessage> channel, ILogger<EmailQueueProcessor> logger)
        {
            _channel = channel;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var msg in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    // TODO: Integrate provider SDK (e.g., SES/SendGrid) based on configuration
                    _logger.LogInformation("Sending email to {To} with subject {Subject}", msg.To, msg.Subject);
                    await Task.Delay(10, stoppingToken); // simulate send
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {To}", msg.To);
                }
            }
        }
    }

    public static class EmailQueueExtensions
    {
        public static IServiceCollection AddEmailQueue(this IServiceCollection services)
        {
            var channel = Channel.CreateUnbounded<EmailMessage>();
            services.AddSingleton(channel);
            services.AddSingleton<IEmailQueue, InMemoryEmailQueue>();
            services.AddHostedService<EmailQueueProcessor>();
            return services;
        }
    }
}
