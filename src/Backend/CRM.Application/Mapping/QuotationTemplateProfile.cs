using AutoMapper;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;

namespace CRM.Application.Mapping
{
    public class QuotationTemplateProfile : Profile
    {
        public QuotationTemplateProfile()
        {
            // QuotationTemplate -> QuotationTemplateDto
            CreateMap<QuotationTemplate, QuotationTemplateDto>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility.ToString()))
                .ForMember(dest => dest.OwnerUserName, opt => opt.MapFrom(src => src.OwnerUser != null ? src.OwnerUser.GetFullName() : string.Empty))
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.GetFullName() : null));

            // QuotationTemplateLineItem -> TemplateLineItemDto
            CreateMap<QuotationTemplateLineItem, TemplateLineItemDto>();

            // CreateQuotationTemplateRequest -> QuotationTemplate
            CreateMap<CreateQuotationTemplateRequest, QuotationTemplate>()
                .ForMember(dest => dest.TemplateId, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerUserId, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerRole, opt => opt.Ignore())
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<TemplateVisibility>(src.Visibility)))
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousVersionId, opt => opt.Ignore())
                .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
                .ForMember(dest => dest.LastUsedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerUser, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousVersion, opt => opt.Ignore())
                .ForMember(dest => dest.LineItems, opt => opt.Ignore());

            // CreateTemplateLineItemRequest -> QuotationTemplateLineItem
            CreateMap<CreateTemplateLineItemRequest, QuotationTemplateLineItem>()
                .ForMember(dest => dest.LineItemId, opt => opt.Ignore())
                .ForMember(dest => dest.TemplateId, opt => opt.Ignore())
                .ForMember(dest => dest.SequenceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Template, opt => opt.Ignore());

            // UpdateTemplateLineItemRequest -> QuotationTemplateLineItem
            // Note: handler controls LineItemId (preserve existing or new Guid), SequenceNumber, Amount, CreatedAt
            CreateMap<UpdateTemplateLineItemRequest, QuotationTemplateLineItem>()
                .ForMember(dest => dest.LineItemId, opt => opt.Ignore())
                .ForMember(dest => dest.TemplateId, opt => opt.Ignore())
                .ForMember(dest => dest.SequenceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Template, opt => opt.Ignore());

            // UpdateQuotationTemplateRequest -> QuotationTemplate (for updates)
            CreateMap<UpdateQuotationTemplateRequest, QuotationTemplate>()
                .ForMember(dest => dest.TemplateId, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerUserId, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerRole, opt => opt.Ignore())
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Visibility) ? Enum.Parse<TemplateVisibility>(src.Visibility) : (TemplateVisibility?)null))
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousVersionId, opt => opt.Ignore())
                .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
                .ForMember(dest => dest.LastUsedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerUser, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousVersion, opt => opt.Ignore())
                .ForMember(dest => dest.LineItems, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // QuotationTemplate -> QuotationTemplateVersionDto
            CreateMap<QuotationTemplate, QuotationTemplateVersionDto>()
                .ForMember(dest => dest.UpdatedByUserName, opt => opt.MapFrom(src => src.OwnerUser != null ? src.OwnerUser.GetFullName() : string.Empty))
                .ForMember(dest => dest.IsCurrentVersion, opt => opt.Ignore());
        }
    }
}

