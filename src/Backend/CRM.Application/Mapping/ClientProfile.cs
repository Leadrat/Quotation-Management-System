using AutoMapper;
using CRM.Application.Clients.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<Client, ClientDto>()
                .ForMember(d => d.CreatedByUserName, o => o.MapFrom(s => 
                    s.CreatedByUser != null 
                        ? $"{s.CreatedByUser.FirstName ?? ""} {s.CreatedByUser.LastName ?? ""}".Trim()
                        : ""))
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.GetDisplayName()));
        }
    }
}
