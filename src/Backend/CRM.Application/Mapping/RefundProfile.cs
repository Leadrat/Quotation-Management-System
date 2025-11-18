using AutoMapper;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class RefundProfile : Profile
    {
        public RefundProfile()
        {
            CreateMap<Refund, RefundDto>()
                .ForMember(dest => dest.RequestedByUserName, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.Ignore());

            CreateMap<RefundTimeline, RefundTimelineDto>()
                .ForMember(dest => dest.ActedByUserName, opt => opt.Ignore());

            CreateMap<Adjustment, AdjustmentDto>()
                .ForMember(dest => dest.RequestedByUserName, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.Ignore())
                .ForMember(dest => dest.AdjustmentDifference, opt => opt.MapFrom(src => src.GetAdjustmentDifference()));
        }
    }
}

