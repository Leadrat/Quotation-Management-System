using AutoMapper;
using CRM.Application.Notifications.DTOs;
using CRM.Domain.Entities;
using System.Text.Json;

namespace CRM.Application.Notifications.Mapping;

public class NotificationTemplateProfile : Profile
{
    public NotificationTemplateProfile()
    {
        CreateMap<NotificationTemplate, NotificationTemplateDto>()
            .ForMember(dest => dest.Variables, opt => opt.MapFrom(src => 
                string.IsNullOrEmpty(src.Variables) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(src.Variables, (JsonSerializerOptions?)null) ?? new List<string>()));

        CreateMap<CreateNotificationTemplateRequest, NotificationTemplate>()
            .ForMember(dest => dest.Variables, opt => opt.MapFrom(src => 
                src.Variables.Count > 0 ? JsonSerializer.Serialize(src.Variables, (JsonSerializerOptions?)null) : null))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateNotificationTemplateRequest, NotificationTemplate>()
            .ForMember(dest => dest.Variables, opt => opt.MapFrom(src => 
                src.Variables.Count > 0 ? JsonSerializer.Serialize(src.Variables, (JsonSerializerOptions?)null) : null))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TemplateKey, opt => opt.Ignore())
            .ForMember(dest => dest.Channel, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}