using AutoMapper;
using CRM.Application.CompanyBankDetails.DTOs;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class CompanyBankDetailsProfile : Profile
    {
        public CompanyBankDetailsProfile()
        {
            CreateMap<BankFieldType, BankFieldTypeDto>();
            
            CreateMap<CountryBankFieldConfiguration, CountryBankFieldConfigurationDto>()
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.CountryName : null))
                .ForMember(dest => dest.BankFieldTypeName, opt => opt.MapFrom(src => src.BankFieldType != null ? src.BankFieldType.Name : null))
                .ForMember(dest => dest.BankFieldTypeDisplayName, opt => opt.MapFrom(src => src.BankFieldType != null ? src.BankFieldType.DisplayName : null));
        }
    }
}

