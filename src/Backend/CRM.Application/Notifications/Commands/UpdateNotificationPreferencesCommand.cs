using System;
using System.Collections.Generic;
using CRM.Application.Notifications.Dtos;
using MediatR;

namespace CRM.Application.Notifications.Commands
{
    public class UpdateNotificationPreferencesCommand : IRequest<NotificationPreferencesDto>
    {
        public Guid UserId { get; set; }
        public Dictionary<string, Dictionary<string, bool>> Preferences { get; set; } = new Dictionary<string, Dictionary<string, bool>>();
    }
}

