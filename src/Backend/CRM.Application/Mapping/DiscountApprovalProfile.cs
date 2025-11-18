using AutoMapper;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;

namespace CRM.Application.Mapping
{
    public class DiscountApprovalProfile : Profile
    {
        public DiscountApprovalProfile()
        {
            // DiscountApproval -> DiscountApprovalDto
            CreateMap<DiscountApproval, DiscountApprovalDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ApprovalLevel, opt => opt.MapFrom(src => src.ApprovalLevel.ToString()))
                .ForMember(dest => dest.QuotationNumber, opt => opt.MapFrom(src => src.Quotation != null ? src.Quotation.QuotationNumber : string.Empty))
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.Quotation != null ? src.Quotation.ClientId : Guid.Empty))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Quotation != null && src.Quotation.Client != null ? src.Quotation.Client.CompanyName : string.Empty))
                .ForMember(dest => dest.RequestedByUserName, opt => opt.MapFrom(src => src.RequestedByUser != null ? src.RequestedByUser.GetFullName() : string.Empty))
                .ForMember(dest => dest.ApproverUserName, opt => opt.MapFrom(src => src.ApproverUser != null ? src.ApproverUser.GetFullName() : null));

            // CreateDiscountApprovalRequest -> DiscountApproval (partial mapping, handler will complete)
            CreateMap<CreateDiscountApprovalRequest, DiscountApproval>()
                .ForMember(dest => dest.ApprovalId, opt => opt.Ignore())
                .ForMember(dest => dest.QuotationId, opt => opt.MapFrom(src => src.QuotationId))
                .ForMember(dest => dest.RequestedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApproverUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.RequestDate, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalDate, opt => opt.Ignore())
                .ForMember(dest => dest.RejectionDate, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentDiscountPercentage, opt => opt.MapFrom(src => src.DiscountPercentage))
                .ForMember(dest => dest.Threshold, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalLevel, opt => opt.Ignore())
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.EscalatedToAdmin, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Quotation, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ApproverUser, opt => opt.Ignore());
        }
    }
}

