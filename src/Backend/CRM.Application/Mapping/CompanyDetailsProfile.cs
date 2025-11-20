using AutoMapper;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class CompanyDetailsProfile : Profile
    {
        public CompanyDetailsProfile()
        {
            CreateMap<Domain.Entities.CompanyDetails, CompanyDetailsDto>();
            
            CreateMap<Domain.Entities.BankDetails, BankDetailsDto>();
            
            CreateMap<BankDetailsDto, Domain.Entities.BankDetails>()
                .ForMember(dest => dest.BankDetailsId, opt => opt.MapFrom(src => 
                    src.BankDetailsId == Guid.Empty ? Guid.NewGuid() : src.BankDetailsId))
                .ForMember(dest => dest.CompanyDetailsId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
        }
    }
}

