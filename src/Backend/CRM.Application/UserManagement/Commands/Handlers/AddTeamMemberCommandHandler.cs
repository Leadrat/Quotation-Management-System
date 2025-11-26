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

public class AddTeamMemberCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public AddTeamMemberCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TeamMemberDto> Handle(AddTeamMemberCommand cmd)
    {
        var team = await _db.Teams
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.TeamId == cmd.TeamId && t.IsActive);

        if (team == null)
        {
            throw new TeamNotFoundException(cmd.TeamId);
        }

        // Authorization: Only Admin, Manager, or Team Lead can add members
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase) ||
                          team.TeamLeadUserId == cmd.AddedByUserId;
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin, Manager, or Team Lead can add team members");
        }

        // Validate user exists and is active
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.Request.UserId);
        if (user == null || !user.IsActive || user.DeletedAt != null)
        {
            throw new InvalidOperationException("User not found or inactive");
        }

        // Check if user is already a member
        var existingMember = await _db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.Request.UserId);
        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a member of this team");
        }

        // Validate role
        var validRoles = new[] { "Member", "Lead", "Admin" };
        if (!validRoles.Contains(cmd.Request.Role))
        {
            throw new InvalidOperationException($"Invalid role. Must be one of: {string.Join(", ", validRoles)}");
        }

        var now = DateTime.UtcNow;
        var teamMember = new TeamMember
        {
            TeamMemberId = Guid.NewGuid(),
            TeamId = cmd.TeamId,
            UserId = cmd.Request.UserId,
            Role = cmd.Request.Role,
            JoinedAt = now
        };

        _db.TeamMembers.Add(teamMember);
        await _db.SaveChangesAsync();

        // Load with navigation properties
        var memberWithNav = await _db.TeamMembers
            .Include(tm => tm.Team)
            .Include(tm => tm.User)
            .FirstOrDefaultAsync(tm => tm.TeamMemberId == teamMember.TeamMemberId);

        return _mapper.Map<TeamMemberDto>(memberWithNav);
    }
}

