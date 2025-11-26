using AutoMapper;
using CRM.Application.Notifications.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<UserNotification, NotificationDto>();
        CreateMap<NotificationType, NotificationTypeDto>();
    }
}
