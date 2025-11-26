using CRM.Domain.Entities;
using AutoMapper;
using CRM.Application.Common.Results;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Notifications.Queries.Handlers;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetUserNotificationsQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        // Clamp pagination parameters
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Min(100, Math.Max(1, request.PageSize));

        var query = _context.Notifications
            .Include(n => n.NotificationType)
            .AsQueryable();

        // Apply user filtering - Admin can view all or specific user's notifications
        if (request.IsAdmin && request.TargetUserId.HasValue)
        {
            query = query.Where(n => n.UserId == request.TargetUserId.Value);
        }
        else if (!request.IsAdmin)
        {
            query = query.Where(n => n.UserId == request.UserId);
        }
        // If Admin and no TargetUserId specified, return all notifications

        // Apply filters
        if (request.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        }

        if (request.NotificationTypeId.HasValue)
        {
            query = query.Where(n => n.NotificationTypeId == request.NotificationTypeId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(n => n.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(n => n.CreatedAt <= request.ToDate.Value);
        }

        // Order by CreatedAt DESC
        query = query.OrderByDescending(n => n.CreatedAt);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var notifications = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var notificationDtos = _mapper.Map<NotificationDto[]>(notifications);

        return new PagedResult<NotificationDto>
        {
            Success = true,
            Data = notificationDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
