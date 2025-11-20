using AutoMapper;
using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class CompanyIdentifiersProfile : Profile
    {
        public CompanyIdentifiersProfile()
        {
            CreateMap<IdentifierType, IdentifierTypeDto>();
            
            CreateMap<CountryIdentifierConfiguration, CountryIdentifierConfigurationDto>()
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.CountryName : null))
                .ForMember(dest => dest.IdentifierTypeName, opt => opt.MapFrom(src => src.IdentifierType != null ? src.IdentifierType.Name : null))
                .ForMember(dest => dest.IdentifierTypeDisplayName, opt => opt.MapFrom(src => src.IdentifierType != null ? src.IdentifierType.DisplayName : null));
        }
    }
}

