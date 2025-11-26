using AutoMapper;
using CRM.Application.Quotations.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;

namespace CRM.Application.Mapping
{
    public class QuotationProfile : Profile
    {
        public QuotationProfile()
        {
            CreateMap<Quotation, QuotationDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.CompanyName))
                .ForMember(dest => dest.ClientEmail, opt => opt.MapFrom(src => src.Client.Email))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToUpper()))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => ResolveUserName(src.CreatedByUser)))
                .ForMember(dest => dest.LineItems, opt => opt.MapFrom(src => src.LineItems));

            CreateMap<QuotationLineItem, LineItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : null));

            CreateMap<CreateLineItemRequest, QuotationLineItem>()
                .ForMember(dest => dest.LineItemId, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationId, opt => opt.Ignore())
                .ForMember(dest => dest.SequenceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Quotation, opt => opt.Ignore())
                .AfterMap((src, dest) => dest.CalculateAmount());

            CreateMap<UpdateLineItemRequest, QuotationLineItem>()
                .ForMember(dest => dest.QuotationId, opt => opt.Ignore())
                .ForMember(dest => dest.SequenceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Quotation, opt => opt.Ignore())
                .AfterMap((src, dest) => dest.CalculateAmount());
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

