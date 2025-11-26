using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class UpdateUserProfileCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public UpdateUserProfileCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<EnhancedUserProfileDto> Handle(UpdateUserProfileCommand cmd)
    {
        // Authorization: Users can only update their own profile
        if (cmd.UpdatedByUserId != cmd.UserId)
        {
            throw new UnauthorizedTeamOperationException("Users can only update their own profile");
        }

        var user = await _db.Users
            .Include(u => u.DelegateUser)
            .FirstOrDefaultAsync(u => u.UserId == cmd.UserId);

        if (user == null || !user.IsActive || user.DeletedAt != null)
        {
            throw new InvalidOperationException("User not found or inactive");
        }

        // Update profile fields
        if (cmd.Request.AvatarUrl != null)
        {
            user.AvatarUrl = cmd.Request.AvatarUrl.Trim();
        }

        if (cmd.Request.Bio != null)
        {
            user.Bio = cmd.Request.Bio.Trim();
        }

        if (cmd.Request.LinkedInUrl != null)
        {
            user.LinkedInUrl = cmd.Request.LinkedInUrl.Trim();
        }

        if (cmd.Request.TwitterUrl != null)
        {
            user.TwitterUrl = cmd.Request.TwitterUrl.Trim();
        }

        if (cmd.Request.Skills != null)
        {
            user.SetSkills(cmd.Request.Skills);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var updatedUser = await _db.Users
            .Include(u => u.DelegateUser)
            .FirstOrDefaultAsync(u => u.UserId == cmd.UserId);

        return _mapper.Map<EnhancedUserProfileDto>(updatedUser);
    }
}

