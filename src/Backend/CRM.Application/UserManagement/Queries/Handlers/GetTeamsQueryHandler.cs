using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class GetTeamsQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetTeamsQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<TeamDto>> Handle(GetTeamsQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize > 100 ? 100 : (query.PageSize < 1 ? 10 : query.PageSize);

        var teamsQuery = _db.Teams
            .AsNoTracking()
            .Include(t => t.TeamLead)
            .Include(t => t.ParentTeam)
            .Include(t => t.Members)
            .Where(t => t.IsActive);

        // Filter by company if provided
        if (query.CompanyId.HasValue)
        {
            teamsQuery = teamsQuery.Where(t => t.CompanyId == query.CompanyId.Value);
        }

        // Filter by team lead if provided
        if (query.TeamLeadUserId.HasValue)
        {
            teamsQuery = teamsQuery.Where(t => t.TeamLeadUserId == query.TeamLeadUserId.Value);
        }

        // Filter by active status if provided
        if (query.IsActive.HasValue)
        {
            teamsQuery = teamsQuery.Where(t => t.IsActive == query.IsActive.Value);
        }

        // Filter by parent team if provided
        if (query.ParentTeamId.HasValue)
        {
            teamsQuery = teamsQuery.Where(t => t.ParentTeamId == query.ParentTeamId.Value);
        }

        var total = await teamsQuery.CountAsync();

        var teams = await teamsQuery
            .OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = teams.Select(t => new TeamDto
        {
            TeamId = t.TeamId,
            Name = t.Name,
            Description = t.Description,
            TeamLeadUserId = t.TeamLeadUserId,
            TeamLeadName = t.TeamLead?.GetFullName() ?? string.Empty,
            ParentTeamId = t.ParentTeamId,
            ParentTeamName = t.ParentTeam?.Name,
            CompanyId = t.CompanyId,
            IsActive = t.IsActive,
            MemberCount = t.Members.Count,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToArray();

        return new PagedResult<TeamDto>
        {
            Success = true,
            Data = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}

