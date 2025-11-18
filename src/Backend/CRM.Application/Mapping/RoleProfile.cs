using AutoMapper;
using CRM.Application.Roles.Queries;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role, RoleDto>()
            .ForMember(d => d.UserCount, opt => opt.Ignore());
    }
}
