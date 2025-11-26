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
                .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => src.Client.ContactName))
                .ForMember(dest => dest.ClientEmail, opt => opt.MapFrom(src => src.Client.Email))
                .ForMember(dest => dest.ClientMobile, opt => opt.MapFrom(src => src.Client.Mobile))
                .ForMember(dest => dest.ClientPhoneCode, opt => opt.MapFrom(src => src.Client.PhoneCode))
                .ForMember(dest => dest.ClientAddress, opt => opt.MapFrom(src => src.Client.Address))
                .ForMember(dest => dest.ClientCity, opt => opt.MapFrom(src => src.Client.City))
                .ForMember(dest => dest.ClientState, opt => opt.MapFrom(src => src.Client.State))
                .ForMember(dest => dest.ClientPinCode, opt => opt.MapFrom(src => src.Client.PinCode))
                .ForMember(dest => dest.ClientGstin, opt => opt.MapFrom(src => src.Client.Gstin))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToUpper()))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()))
                .ForMember(dest => dest.LineItems, opt => opt.MapFrom(src => src.LineItems))
                .ForMember(dest => dest.CompanyDetails, opt => opt.Ignore()); // Set manually in handler from snapshot
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
