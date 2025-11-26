using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class GetTeamMembersQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetTeamMembersQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TeamMemberDto[]> Handle(GetTeamMembersQuery query)
    {
        var team = await _db.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TeamId == query.TeamId && t.IsActive);

        if (team == null)
        {
            throw new TeamNotFoundException(query.TeamId);
        }

        var members = await _db.TeamMembers
            .AsNoTracking()
            .Include(tm => tm.Team)
            .Include(tm => tm.User)
            .Where(tm => tm.TeamId == query.TeamId)
            .OrderBy(tm => tm.JoinedAt)
            .ToArrayAsync();

        return members.Select(m => new TeamMemberDto
        {
            TeamMemberId = m.TeamMemberId,
            TeamId = m.TeamId,
            TeamName = m.Team.Name,
            UserId = m.UserId,
            UserName = m.User.GetFullName(),
            UserEmail = m.User.Email,
            Role = m.Role,
            JoinedAt = m.JoinedAt
        }).ToArray();
    }
}

