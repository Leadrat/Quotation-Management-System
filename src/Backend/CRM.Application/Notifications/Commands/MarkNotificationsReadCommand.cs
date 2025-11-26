using System;
using System.Collections.Generic;
using MediatR;

namespace CRM.Application.Notifications.Commands
{
    public class MarkNotificationsReadCommand : IRequest<int>
    {
        public List<Guid>? NotificationIds { get; set; } // null or empty = mark all
        public Guid RequestedByUserId { get; set; }
    }
}

