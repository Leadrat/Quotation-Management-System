using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class GetActivityFeedQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetActivityFeedQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserActivityDto>> Handle(GetActivityFeedQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize > 100 ? 100 : (query.PageSize < 1 ? 10 : query.PageSize);

        var activitiesQuery = _db.UserActivities
            .AsNoTracking()
            .Include(ua => ua.User)
            .AsQueryable();

        // Filter by user if provided
        if (query.UserId.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(ua => ua.UserId == query.UserId.Value);
        }

        // Filter by action type if provided
        if (!string.IsNullOrWhiteSpace(query.ActionType))
        {
            activitiesQuery = activitiesQuery.Where(ua => ua.ActionType == query.ActionType);
        }

        // Filter by entity type if provided
        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            activitiesQuery = activitiesQuery.Where(ua => ua.EntityType == query.EntityType);
        }

        // Filter by date range
        if (query.FromDate.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(ua => ua.Timestamp >= query.FromDate.Value);
        }
        if (query.ToDate.HasValue)
        {
            activitiesQuery = activitiesQuery.Where(ua => ua.Timestamp <= query.ToDate.Value);
        }

        var total = await activitiesQuery.CountAsync();

        var activities = await activitiesQuery
            .OrderByDescending(ua => ua.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var dtos = activities.Select(a => new UserActivityDto
        {
            ActivityId = a.ActivityId,
            UserId = a.UserId,
            UserName = a.User?.GetFullName() ?? string.Empty,
            ActionType = a.ActionType,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            IpAddress = a.IpAddress,
            UserAgent = a.UserAgent,
            Timestamp = a.Timestamp
        }).ToArray();

        return new PagedResult<UserActivityDto>
        {
            Success = true,
            Data = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}

