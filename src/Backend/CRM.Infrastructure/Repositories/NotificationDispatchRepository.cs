using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Repositories;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class NotificationDispatchRepository : INotificationDispatchRepository
{
    private readonly IAppDbContext _context;

    public NotificationDispatchRepository(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationDispatchAttempt?> GetByIdAsync(int id)
    {
        return await _context.NotificationDispatchAttempts
            .Include(da => da.Notification)
            .FirstOrDefaultAsync(da => da.Id == id);
    }

    public async Task<List<NotificationDispatchAttempt>> GetByNotificationIdAsync(Guid notificationId)
    {
        return await _context.NotificationDispatchAttempts
            .Where(da => da.NotificationId == notificationId)
            .OrderBy(da => da.AttemptedAt)
            .ToListAsync();
    }

    public async Task<List<NotificationDispatchAttempt>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.NotificationDispatchAttempts
            .Include(da => da.Notification)
            .Where(da => da.Notification.UserId == userId)
            .OrderByDescending(da => da.AttemptedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<NotificationDispatchAttempt>> GetFailedAttemptsAsync(NotificationChannel? channel = null, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.NotificationDispatchAttempts
            .Include(da => da.Notification)
            .Where(da => da.Status == DispatchStatus.Failed);

        if (channel.HasValue)
        {
            query = query.Where(da => da.Channel == channel.Value);
        }

        return await query
            .OrderByDescending(da => da.AttemptedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<NotificationDispatchAttempt>> GetPermanentlyFailedAttemptsAsync(NotificationChannel? channel = null, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.NotificationDispatchAttempts
            .Include(da => da.Notification)
            .Where(da => da.Status == DispatchStatus.PermanentlyFailed);

        if (channel.HasValue)
        {
            query = query.Where(da => da.Channel == channel.Value);
        }

        return await query
            .OrderByDescending(da => da.AttemptedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<NotificationDispatchAttempt>> GetDispatchHistoryAsync(
        Guid? notificationId = null,
        Guid? userId = null,
        NotificationChannel? channel = null,
        DispatchStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var query = _context.NotificationDispatchAttempts
            .Include(da => da.Notification)
                .ThenInclude(n => n.User)
            .AsQueryable();

        if (notificationId.HasValue)
            query = query.Where(da => da.NotificationId == notificationId.Value);

        if (userId.HasValue)
            query = query.Where(da => da.Notification.UserId == userId.Value);

        if (channel.HasValue)
            query = query.Where(da => da.Channel == channel.Value);

        if (status.HasValue)
            query = query.Where(da => da.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(da => da.AttemptedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(da => da.AttemptedAt <= toDate.Value);

        return await query
            .OrderByDescending(da => da.AttemptedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetDispatchHistoryCountAsync(
        Guid? notificationId = null,
        Guid? userId = null,
        NotificationChannel? channel = null,
        DispatchStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.NotificationDispatchAttempts
            .Include(da => da.Notification)
            .AsQueryable();

        if (notificationId.HasValue)
            query = query.Where(da => da.NotificationId == notificationId.Value);

        if (userId.HasValue)
            query = query.Where(da => da.Notification.UserId == userId.Value);

        if (channel.HasValue)
            query = query.Where(da => da.Channel == channel.Value);

        if (status.HasValue)
            query = query.Where(da => da.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(da => da.AttemptedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(da => da.AttemptedAt <= toDate.Value);

        return await query.CountAsync();
    }

    public async Task<NotificationDispatchAttempt> CreateAsync(NotificationDispatchAttempt attempt)
    {
        attempt.AttemptedAt = DateTime.UtcNow;
        _context.NotificationDispatchAttempts.Add(attempt);
        await _context.SaveChangesAsync();
        return attempt;
    }

    public async Task<NotificationDispatchAttempt> UpdateAsync(NotificationDispatchAttempt attempt)
    {
        _context.NotificationDispatchAttempts.Update(attempt);
        await _context.SaveChangesAsync();
        return attempt;
    }

    public async Task DeleteAsync(int id)
    {
        var attempt = await GetByIdAsync(id);
        if (attempt != null)
        {
            _context.NotificationDispatchAttempts.Remove(attempt);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<string, int>> GetDispatchStatisticsByChannelAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.NotificationDispatchAttempts.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(da => da.AttemptedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(da => da.AttemptedAt <= toDate.Value);

        return await query
            .GroupBy(da => da.Channel)
            .Select(g => new { Channel = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Channel, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetDispatchStatisticsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.NotificationDispatchAttempts.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(da => da.AttemptedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(da => da.AttemptedAt <= toDate.Value);

        return await query
            .GroupBy(da => da.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    public async Task<double> GetAverageDeliveryTimeAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.NotificationDispatchAttempts
            .Where(da => da.Status == DispatchStatus.Delivered && da.DeliveredAt.HasValue);

        if (fromDate.HasValue)
            query = query.Where(da => da.AttemptedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(da => da.AttemptedAt <= toDate.Value);

        var deliveryTimes = await query
            .Select(da => new { 
                AttemptedAt = da.AttemptedAt, 
                DeliveredAt = da.DeliveredAt!.Value 
            })
            .ToListAsync();

        if (!deliveryTimes.Any())
            return 0;

        var totalMinutes = deliveryTimes
            .Sum(dt => (dt.DeliveredAt - dt.AttemptedAt).TotalMinutes);

        return totalMinutes / deliveryTimes.Count;
    }

    public async Task<int> GetTotalAttemptsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.NotificationDispatchAttempts.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(da => da.AttemptedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(da => da.AttemptedAt <= toDate.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetSuccessfulDeliveriesAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.NotificationDispatchAttempts
            .Where(da => da.Status == DispatchStatus.Delivered);

        if (fromDate.HasValue)
            query = query.Where(da => da.AttemptedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(da => da.AttemptedAt <= toDate.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetFailedAttemptsCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.NotificationDispatchAttempts
            .Where(da => da.Status == DispatchStatus.Failed || da.Status == DispatchStatus.PermanentlyFailed);

        if (fromDate.HasValue)
            query = query.Where(da => da.AttemptedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(da => da.AttemptedAt <= toDate.Value);

        return await query.CountAsync();
    }
}