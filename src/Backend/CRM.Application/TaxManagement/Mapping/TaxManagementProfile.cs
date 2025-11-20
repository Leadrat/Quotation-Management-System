using AutoMapper;
using CRM.Application.TaxManagement.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.TaxManagement.Mapping
{
    public class TaxManagementProfile : Profile
    {
        public TaxManagementProfile()
        {
            CreateMap<Country, CountryDto>();

            CreateMap<TaxFramework, TaxFrameworkDto>()
                .ForMember(d => d.TaxComponents, o => o.MapFrom(s => s.GetTaxComponents()));

            CreateMap<Jurisdiction, JurisdictionDto>()
                .ForMember(d => d.CountryName, o => o.Ignore())
                .ForMember(d => d.ParentJurisdictionName, o => o.Ignore());

            CreateMap<ProductServiceCategory, ProductServiceCategoryDto>();

            CreateMap<TaxRate, TaxRateDto>()
                .ForMember(d => d.TaxRate, o => o.MapFrom(s => s.Rate)) // Map Rate property to TaxRate DTO property
                .ForMember(d => d.TaxComponents, o => o.Ignore()) // Set manually in handlers
                .ForMember(d => d.JurisdictionName, o => o.Ignore())
                .ForMember(d => d.CategoryName, o => o.Ignore())
                .ForMember(d => d.FrameworkName, o => o.Ignore());

            CreateMap<TaxCalculationLog, TaxCalculationLogDto>()
                .ForMember(d => d.CalculationDetails, o => o.Ignore()) // Deserialize manually
                .ForMember(d => d.ChangedByUserName, o => o.Ignore())
                .ForMember(d => d.CountryName, o => o.Ignore())
                .ForMember(d => d.JurisdictionName, o => o.Ignore());
        }
    }
}

