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

public class UpdateTeamCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public UpdateTeamCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TeamDto> Handle(UpdateTeamCommand cmd)
    {
        var team = await _db.Teams
            .Include(t => t.TeamLead)
            .Include(t => t.ParentTeam)
            .FirstOrDefaultAsync(t => t.TeamId == cmd.TeamId);

        if (team == null || !team.IsActive)
        {
            throw new TeamNotFoundException(cmd.TeamId);
        }

        // Authorization: Only Admin, Manager, or Team Lead can update
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase) ||
                          team.TeamLeadUserId == cmd.UpdatedByUserId;
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin, Manager, or Team Lead can update teams");
        }

        // Update name if provided and check uniqueness
        if (!string.IsNullOrWhiteSpace(cmd.Request.Name))
        {
            var nameLower = cmd.Request.Name.Trim().ToLowerInvariant();
            var exists = await _db.Teams.AnyAsync(t => 
                t.TeamId != cmd.TeamId &&
                t.CompanyId == team.CompanyId && 
                t.Name.ToLower() == nameLower && 
                t.IsActive);
            if (exists)
            {
                throw new DuplicateTeamNameException(cmd.Request.Name);
            }
            team.Name = cmd.Request.Name.Trim();
        }

        if (cmd.Request.Description != null)
        {
            team.Description = cmd.Request.Description.Trim();
        }

        if (cmd.Request.TeamLeadUserId.HasValue)
        {
            var newTeamLead = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.Request.TeamLeadUserId.Value);
            if (newTeamLead == null || !newTeamLead.IsActive || newTeamLead.DeletedAt != null)
            {
                throw new InvalidOperationException("Team lead user not found or inactive");
            }
            team.TeamLeadUserId = cmd.Request.TeamLeadUserId.Value;
        }

        if (cmd.Request.ParentTeamId.HasValue)
        {
            if (cmd.Request.ParentTeamId.Value == cmd.TeamId)
            {
                throw new InvalidOperationException("Team cannot be its own parent");
            }
            var parentTeam = await _db.Teams.FirstOrDefaultAsync(t => t.TeamId == cmd.Request.ParentTeamId.Value);
            if (parentTeam == null || !parentTeam.IsActive)
            {
                throw new InvalidOperationException("Parent team not found or inactive");
            }
            if (parentTeam.CompanyId != team.CompanyId)
            {
                throw new InvalidOperationException("Parent team must belong to the same company");
            }
            team.ParentTeamId = cmd.Request.ParentTeamId.Value;
        }

        if (cmd.Request.IsActive.HasValue)
        {
            team.IsActive = cmd.Request.IsActive.Value;
        }

        team.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var updatedTeam = await _db.Teams
            .Include(t => t.TeamLead)
            .Include(t => t.ParentTeam)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.TeamId == team.TeamId);

        var dto = _mapper.Map<TeamDto>(updatedTeam);
        dto.MemberCount = updatedTeam?.Members.Count ?? 0;

        return dto;
    }
}

