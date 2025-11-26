using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Repositories;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly IAppDbContext _context;

    public NotificationTemplateRepository(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate?> GetByIdAsync(int id)
    {
        return await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<NotificationTemplate?> GetByTemplateKeyAsync(string templateKey)
    {
        return await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey);
    }

    public async Task<List<NotificationTemplate>> GetByChannelAsync(NotificationChannel channel, bool activeOnly = true)
    {
        var query = _context.NotificationTemplates
            .Where(t => t.Channel == channel);

        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<NotificationTemplate>> GetAllAsync(bool activeOnly = true)
    {
        var query = _context.NotificationTemplates.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderBy(t => t.Channel)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<NotificationTemplate> CreateAsync(NotificationTemplate template)
    {
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        _context.NotificationTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<NotificationTemplate> UpdateAsync(NotificationTemplate template)
    {
        template.UpdatedAt = DateTime.UtcNow;

        _context.NotificationTemplates.Update(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task DeleteAsync(int id)
    {
        var template = await GetByIdAsync(id);
        if (template != null)
        {
            _context.NotificationTemplates.Remove(template);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string templateKey)
    {
        return await _context.NotificationTemplates
            .AnyAsync(t => t.TemplateKey == templateKey);
    }

    public async Task<List<NotificationTemplate>> SearchAsync(string searchTerm, NotificationChannel? channel = null, bool activeOnly = true)
    {
        var query = _context.NotificationTemplates.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        if (channel.HasValue)
        {
            query = query.Where(t => t.Channel == channel.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => 
                t.Name.Contains(searchTerm) ||
                t.TemplateKey.Contains(searchTerm) ||
                (t.Description != null && t.Description.Contains(searchTerm)));
        }

        return await query
            .OrderBy(t => t.Channel)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<NotificationTemplate>> GetByEventTypeAsync(string eventType)
    {
        return await _context.NotificationTemplates
            .Where(t => t.EventType == eventType && t.IsActive)
            .OrderBy(t => t.Channel)
            .ToListAsync();
    }

    public async Task<List<string>> GetVariablesForEventTypeAsync(string eventType)
    {
        var templates = await _context.NotificationTemplates
            .Where(t => t.EventType == eventType && t.IsActive)
            .Select(t => t.RequiredVariables)
            .ToListAsync();

        return templates
            .SelectMany(vars => vars ?? new List<string>())
            .Distinct()
            .OrderBy(v => v)
            .ToList();
    }
}