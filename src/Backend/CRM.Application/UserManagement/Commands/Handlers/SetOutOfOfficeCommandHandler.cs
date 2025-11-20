using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class SetOutOfOfficeCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public SetOutOfOfficeCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<EnhancedUserProfileDto> Handle(SetOutOfOfficeCommand cmd)
    {
        // Authorization: Users can only set their own OOO status
        if (cmd.UpdatedByUserId != cmd.UserId)
        {
            throw new UnauthorizedTeamOperationException("Users can only set their own out-of-office status");
        }

        var user = await _db.Users
            .Include(u => u.DelegateUser)
            .FirstOrDefaultAsync(u => u.UserId == cmd.UserId);

        if (user == null || !user.IsActive || user.DeletedAt != null)
        {
            throw new InvalidOperationException("User not found or inactive");
        }

        // Validate delegate user if provided
        if (cmd.Request.DelegateUserId.HasValue)
        {
            var delegateUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.Request.DelegateUserId.Value);
            if (delegateUser == null || !delegateUser.IsActive || delegateUser.DeletedAt != null)
            {
                throw new InvalidOperationException("Delegate user not found or inactive");
            }
        }

        user.SetOutOfOffice(
            cmd.Request.IsOutOfOffice,
            cmd.Request.Message?.Trim(),
            cmd.Request.DelegateUserId
        );

        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var updatedUser = await _db.Users
            .Include(u => u.DelegateUser)
            .FirstOrDefaultAsync(u => u.UserId == cmd.UserId);

        return _mapper.Map<EnhancedUserProfileDto>(updatedUser);
    }
}

