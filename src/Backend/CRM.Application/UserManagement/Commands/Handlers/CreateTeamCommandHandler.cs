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

public class CreateTeamCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public CreateTeamCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TeamDto> Handle(CreateTeamCommand cmd)
    {
        // Authorization: Only Admin or Manager can create teams
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin or Manager can create teams");
        }

        // Validate team name uniqueness per company
        var nameLower = cmd.Request.Name.Trim().ToLowerInvariant();
        var exists = await _db.Teams.AnyAsync(t => 
            t.CompanyId == cmd.Request.CompanyId && 
            t.Name.ToLower() == nameLower && 
            t.IsActive);
        if (exists)
        {
            throw new DuplicateTeamNameException(cmd.Request.Name);
        }

        // Validate team lead exists
        var teamLead = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.Request.TeamLeadUserId);
        if (teamLead == null || !teamLead.IsActive || teamLead.DeletedAt != null)
        {
            throw new InvalidOperationException("Team lead user not found or inactive");
        }

        // Validate parent team if provided
        if (cmd.Request.ParentTeamId.HasValue)
        {
            var parentTeam = await _db.Teams.FirstOrDefaultAsync(t => t.TeamId == cmd.Request.ParentTeamId.Value);
            if (parentTeam == null || !parentTeam.IsActive)
            {
                throw new InvalidOperationException("Parent team not found or inactive");
            }
            if (parentTeam.CompanyId != cmd.Request.CompanyId)
            {
                throw new InvalidOperationException("Parent team must belong to the same company");
            }
        }

        var now = DateTime.UtcNow;
        var team = new Team
        {
            TeamId = Guid.NewGuid(),
            Name = cmd.Request.Name.Trim(),
            Description = cmd.Request.Description?.Trim(),
            TeamLeadUserId = cmd.Request.TeamLeadUserId,
            ParentTeamId = cmd.Request.ParentTeamId,
            CompanyId = cmd.Request.CompanyId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Teams.Add(team);
        await _db.SaveChangesAsync();

        // Load team with navigation properties for DTO mapping
        var teamWithNav = await _db.Teams
            .Include(t => t.TeamLead)
            .Include(t => t.ParentTeam)
            .FirstOrDefaultAsync(t => t.TeamId == team.TeamId);

        var dto = _mapper.Map<TeamDto>(teamWithNav);
        dto.MemberCount = 0; // No members yet

        return dto;
    }
}

