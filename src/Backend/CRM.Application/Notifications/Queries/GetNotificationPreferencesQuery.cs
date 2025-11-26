using System;
using CRM.Application.Notifications.Dtos;
using MediatR;

namespace CRM.Application.Notifications.Queries
{
    public class GetNotificationPreferencesQuery : IRequest<NotificationPreferencesDto>
    {
        public Guid RequestorUserId { get; set; }
    }
}

