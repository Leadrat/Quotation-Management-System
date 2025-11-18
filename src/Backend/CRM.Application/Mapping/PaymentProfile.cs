using AutoMapper;
using CRM.Application.Payments.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.StatusLabel, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
                .ForMember(dest => dest.CanBeRefunded, opt => opt.MapFrom(src => src.CanBeRefunded()))
                .ForMember(dest => dest.CanBeCancelled, opt => opt.MapFrom(src => src.CanBeCancelled()));

            CreateMap<PaymentGatewayConfig, PaymentGatewayConfigDto>();
        }
    }
}

