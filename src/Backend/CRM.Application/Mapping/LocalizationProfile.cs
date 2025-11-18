using AutoMapper;
using CRM.Application.Localization.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping;

public class LocalizationProfile : Profile
{
    public LocalizationProfile()
    {
        CreateMap<Currency, CurrencyDto>();
        CreateMap<ExchangeRate, ExchangeRateDto>();
        CreateMap<UserPreferences, UserPreferencesDto>();
        CreateMap<CompanyPreferences, CompanyPreferencesDto>();
        CreateMap<LocalizationResource, LocalizationResourceDto>();
        CreateMap<SupportedLanguage, SupportedLanguageDto>();
    }
}

