using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Main service for dispatching notifications through various channels
/// </summary>
public class NotificationDispatchService : INotificationDispatchService
{
    private readonly IAppDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationDispatchService> _logger;
    private readonly Dictionary<NotificationChannel, Type> _channelServices;

    public NotificationDispatchService(
        IAppDbContext context,
        IServiceProvider serviceProvider,
        ILogger<NotificationDispatchService> logger)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Map channels to their dispatch services
        _channelServices = new Dictionary<NotificationChannel, Type>
        {
            { NotificationChannel.InApp, typeof(InAppNotificationDispatchService) },
            { NotificationChannel.Email, typeof(EmailNotificationDispatchService) },
            { NotificationChannel.SMS, typeof(SmsNotificationDispatchService) }
        };
    }

    public async Task DispatchNotificationAsync(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

        if (notification != null)
        {
            await DispatchNotificationAsync(notification);
        }
    }

    public async Task DispatchNotificationAsync(UserNotification notification)
    {
        // Dispatch to all enabled channels for this notification type
        var channels = new[] { NotificationChannel.InApp, NotificationChannel.Email };
        
        foreach (var channel in channels)
        {
            var channelService = GetChannelService(channel);
            if (channelService != null)
            {
                var canDispatch = await channelService.CanDispatchAsync(notification);
                if (canDispatch)
                {
                    await channelService.DispatchAsync(notification);
                }
            }
        }
    }

    public async Task<bool> CanDispatchToChannelAsync(NotificationChannel channel)
    {
        var channelService = GetChannelService(channel);
        return channelService != null && await channelService.IsChannelEnabledAsync();
    }

    public async Task<IEnumerable<NotificationDispatchAttempt>> GetDispatchHistoryAsync(Guid notificationId)
    {
        return await _context.NotificationDispatchAttempts
            .Where(da => da.NotificationId == notificationId)
            .OrderBy(da => da.AttemptedAt)
            .ToListAsync();
    }

    public async Task RetryFailedDispatchAsync(int dispatchAttemptId)
    {
        var failedAttempt = await _context.NotificationDispatchAttempts
            .Include(da => da.Notification)
            .FirstOrDefaultAsync(da => da.Id == dispatchAttemptId && da.Status == DispatchStatus.Failed);

        if (failedAttempt?.Notification != null)
        {
            var channelService = GetChannelService(failedAttempt.Channel);
            if (channelService != null)
            {
                var canDispatch = await channelService.CanDispatchAsync(failedAttempt.Notification);
                if (canDispatch)
                {
                    await channelService.DispatchAsync(failedAttempt.Notification);
                }
            }
        }
    }



    private IChannelDispatchService? GetChannelService(NotificationChannel channel)
    {
        if (!_channelServices.TryGetValue(channel, out var serviceType))
        {
            return null;
        }

        return _serviceProvider.GetService(serviceType) as IChannelDispatchService;
    }
}