using AutoMapper;
using CRM.Domain.Entities;
using CRM.Shared.DTOs;
using CRM.Application.Users.Dtos;

namespace CRM.Application.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.RoleName : null))
            .ForMember(d => d.ReportingManagerName, o => o.MapFrom(s => s.ReportingManager != null ? (s.ReportingManager.FirstName + " " + s.ReportingManager.LastName) : null));

        CreateMap<User, UserSummary>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId));
    }
}
