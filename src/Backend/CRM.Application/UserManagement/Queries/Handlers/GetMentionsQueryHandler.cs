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

public class GetMentionsQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetMentionsQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<MentionDto>> Handle(GetMentionsQuery query)
    {
        // Authorization: Users can only view their own mentions
        if (query.RequestorUserId != query.UserId)
        {
            throw new UnauthorizedAccessException("Not authorized to view this user's mentions");
        }

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize > 100 ? 100 : (query.PageSize < 1 ? 10 : query.PageSize);

        var mentionsQuery = _db.Mentions
            .AsNoTracking()
            .Include(m => m.MentionedUser)
            .Include(m => m.MentionedByUser)
            .Where(m => m.MentionedUserId == query.UserId);

        // Filter by read status if provided
        if (query.IsRead.HasValue)
        {
            mentionsQuery = mentionsQuery.Where(m => m.IsRead == query.IsRead.Value);
        }

        var total = await mentionsQuery.CountAsync();

        var mentions = await mentionsQuery
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var dtos = mentions.Select(m => new MentionDto
        {
            MentionId = m.MentionId,
            EntityType = m.EntityType,
            EntityId = m.EntityId,
            MentionedUserId = m.MentionedUserId,
            MentionedUserName = m.MentionedUser?.GetFullName() ?? string.Empty,
            MentionedByUserId = m.MentionedByUserId,
            MentionedByUserName = m.MentionedByUser?.GetFullName() ?? string.Empty,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToArray();

        return new PagedResult<MentionDto>
        {
            Success = true,
            Data = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}

