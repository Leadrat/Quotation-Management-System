using AutoMapper;
using CRM.Application.Quotations.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class QuotationManagementProfile : Profile
    {
        public QuotationManagementProfile()
        {
            CreateMap<QuotationAccessLink, QuotationAccessLinkDto>()
                .ForMember(dest => dest.ViewUrl, opt => opt.Ignore()); // Will be set in handler

            CreateMap<QuotationStatusHistory, QuotationStatusHistoryDto>()
                .ForMember(dest => dest.ChangedByUserName, opt => opt.MapFrom(src => ResolveUserName(src.ChangedByUser)));

            CreateMap<QuotationResponse, QuotationResponseDto>();

            CreateMap<Quotation, PublicQuotationDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.CompanyName))
                .ForMember(dest => dest.ClientEmail, opt => opt.MapFrom(src => src.Client.Email))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToUpper()))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()))
                .ForMember(dest => dest.LineItems, opt => opt.MapFrom(src => src.LineItems));
        }

        private static string ResolveUserName(User? user)
        {
            if (user == null)
            {
                return "System";
            }

            var first = user.FirstName ?? string.Empty;
            var last = user.LastName ?? string.Empty;
            var full = $"{first} {last}".Trim();
            return string.IsNullOrWhiteSpace(full) ? user.Email : full;
        }
    }
}
