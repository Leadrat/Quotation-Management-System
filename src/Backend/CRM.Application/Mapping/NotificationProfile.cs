using AutoMapper;
using CRM.Application.Notifications.Dtos;
using CRM.Domain.Entities;
using System.Text.Json;

namespace CRM.Application.Mapping
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            // Notification -> NotificationDto
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.DeliveryStatus, opt => opt.MapFrom(src => src.DeliveryStatus));

            // NotificationPreference -> NotificationPreferencesDto
            CreateMap<NotificationPreference, NotificationPreferencesDto>()
                .AfterMap((src, dest) =>
                {
                    if (string.IsNullOrWhiteSpace(src.PreferenceData) || src.PreferenceData == "{}")
                    {
                        dest.Preferences = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, bool>>();
                    }
                    else
                    {
                        dest.Preferences = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, bool>>>(src.PreferenceData)
                            ?? new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, bool>>();
                    }
                });

            // EmailNotificationLog -> EmailNotificationLogDto
            CreateMap<EmailNotificationLog, EmailNotificationLogDto>();
        }
    }
}

