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

public class GetTeamByIdQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetTeamByIdQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TeamDto> Handle(GetTeamByIdQuery query)
    {
        var team = await _db.Teams
            .AsNoTracking()
            .Include(t => t.TeamLead)
            .Include(t => t.ParentTeam)
            .Include(t => t.Members)
            .Include(t => t.ChildTeams)
            .FirstOrDefaultAsync(t => t.TeamId == query.TeamId);

        if (team == null)
        {
            throw new TeamNotFoundException(query.TeamId);
        }

        var dto = new TeamDto
        {
            TeamId = team.TeamId,
            Name = team.Name,
            Description = team.Description,
            TeamLeadUserId = team.TeamLeadUserId,
            TeamLeadName = team.TeamLead?.GetFullName() ?? string.Empty,
            ParentTeamId = team.ParentTeamId,
            ParentTeamName = team.ParentTeam?.Name,
            CompanyId = team.CompanyId,
            IsActive = team.IsActive,
            MemberCount = team.Members.Count,
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt,
            ChildTeams = team.ChildTeams.Select(ct => new TeamDto
            {
                TeamId = ct.TeamId,
                Name = ct.Name,
                Description = ct.Description,
                TeamLeadUserId = ct.TeamLeadUserId,
                ParentTeamId = ct.ParentTeamId,
                CompanyId = ct.CompanyId,
                IsActive = ct.IsActive,
                CreatedAt = ct.CreatedAt,
                UpdatedAt = ct.UpdatedAt
            }).ToList()
        };

        return dto;
    }
}

