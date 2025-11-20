using AutoMapper;
using CRM.Application.UserManagement.DTOs;
using CRM.Domain.UserManagement;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping;

public class UserManagementProfile : Profile
{
    public UserManagementProfile()
    {
        CreateMap<Team, TeamDto>()
            .ForMember(d => d.TeamLeadName, o => o.MapFrom(s => s.TeamLead != null ? s.TeamLead.GetFullName() : string.Empty))
            .ForMember(d => d.ParentTeamName, o => o.MapFrom(s => s.ParentTeam != null ? s.ParentTeam.Name : null))
            .ForMember(d => d.MemberCount, o => o.MapFrom(s => s.Members.Count))
            .ForMember(d => d.ChildTeams, o => o.Ignore()); // Handled manually in query handlers

        CreateMap<TeamMember, TeamMemberDto>()
            .ForMember(d => d.TeamName, o => o.MapFrom(s => s.Team != null ? s.Team.Name : string.Empty))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.GetFullName() : string.Empty))
            .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User != null ? s.User.Email : string.Empty));

        CreateMap<UserGroup, UserGroupDto>()
            .ForMember(d => d.CreatedByUserName, o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.GetFullName() : string.Empty))
            .ForMember(d => d.Permissions, o => o.MapFrom(s => s.GetPermissions()))
            .ForMember(d => d.MemberCount, o => o.MapFrom(s => s.Members.Count));

        CreateMap<UserGroupMember, UserGroupMemberDto>()
            .ForMember(d => d.GroupName, o => o.MapFrom(s => s.UserGroup != null ? s.UserGroup.Name : string.Empty))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.GetFullName() : string.Empty))
            .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User != null ? s.User.Email : string.Empty));

        CreateMap<TaskAssignment, TaskAssignmentDto>()
            .ForMember(d => d.AssignedToUserName, o => o.MapFrom(s => s.AssignedToUser != null ? s.AssignedToUser.GetFullName() : string.Empty))
            .ForMember(d => d.AssignedByUserName, o => o.MapFrom(s => s.AssignedByUser != null ? s.AssignedByUser.GetFullName() : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.IsOverdue, o => o.MapFrom(s => s.IsOverdue()));

        CreateMap<Mention, MentionDto>()
            .ForMember(d => d.MentionedUserName, o => o.MapFrom(s => s.MentionedUser != null ? s.MentionedUser.GetFullName() : string.Empty))
            .ForMember(d => d.MentionedByUserName, o => o.MapFrom(s => s.MentionedByUser != null ? s.MentionedByUser.GetFullName() : string.Empty));

        CreateMap<User, EnhancedUserProfileDto>()
            .ForMember(d => d.DelegateUserName, o => o.MapFrom(s => s.DelegateUser != null ? s.DelegateUser.GetFullName() : null))
            .ForMember(d => d.Skills, o => o.MapFrom(s => s.GetSkills()));
    }
}

