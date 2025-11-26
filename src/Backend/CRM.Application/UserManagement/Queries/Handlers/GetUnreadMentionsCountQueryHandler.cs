using System;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Queries;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class GetUnreadMentionsCountQueryHandler
{
    private readonly IAppDbContext _db;

    public GetUnreadMentionsCountQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<int> Handle(GetUnreadMentionsCountQuery query)
    {
        // Authorization: Users can only view their own mention count
        if (query.RequestorUserId != query.UserId)
        {
            throw new UnauthorizedAccessException("Not authorized to view this user's mention count");
        }

        var count = await _db.Mentions
            .AsNoTracking()
            .CountAsync(m => m.MentionedUserId == query.UserId && !m.IsRead);

        return count;
    }
}

