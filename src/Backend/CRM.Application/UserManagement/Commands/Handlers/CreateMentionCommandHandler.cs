using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using CRM.Domain.UserManagement;
using CRM.Domain.UserManagement.Events;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class CreateMentionCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public CreateMentionCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<MentionDto> Handle(CreateMentionCommand cmd)
    {
        // Validate mentioned user exists and is active
        var mentionedUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.Request.MentionedUserId);
        if (mentionedUser == null || !mentionedUser.IsActive || mentionedUser.DeletedAt != null)
        {
            throw new InvalidOperationException("Mentioned user not found or inactive");
        }

        // Validate entity type
        var validEntityTypes = new[] { "Comment", "Note" };
        if (!validEntityTypes.Contains(cmd.Request.EntityType))
        {
            throw new InvalidOperationException($"Invalid entity type. Must be one of: {string.Join(", ", validEntityTypes)}");
        }

        // TODO: Validate entity exists (check Comments, Notes tables based on EntityType)

        var now = DateTime.UtcNow;
        var mention = new Mention
        {
            MentionId = Guid.NewGuid(),
            EntityType = cmd.Request.EntityType,
            EntityId = cmd.Request.EntityId,
            MentionedUserId = cmd.Request.MentionedUserId,
            MentionedByUserId = cmd.MentionedByUserId,
            IsRead = false,
            CreatedAt = now
        };

        _db.Mentions.Add(mention);
        await _db.SaveChangesAsync();

        // Load with navigation properties
        var mentionWithNav = await _db.Mentions
            .Include(m => m.MentionedUser)
            .Include(m => m.MentionedByUser)
            .FirstOrDefaultAsync(m => m.MentionId == mention.MentionId);

        return _mapper.Map<MentionDto>(mentionWithNav);
    }
}

