using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class MarkMentionReadCommandHandler
{
    private readonly IAppDbContext _db;

    public MarkMentionReadCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(MarkMentionReadCommand cmd)
    {
        var mention = await _db.Mentions
            .FirstOrDefaultAsync(m => m.MentionId == cmd.MentionId);

        if (mention == null)
        {
            throw new InvalidOperationException("Mention not found");
        }

        // Authorization: Only the mentioned user can mark as read
        if (mention.MentionedUserId != cmd.UserId)
        {
            throw new UnauthorizedTeamOperationException("Only the mentioned user can mark mention as read");
        }

        mention.MarkAsRead();
        await _db.SaveChangesAsync();
    }
}

