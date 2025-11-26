using System;
using CRM.Application.Notifications.Dtos;
using MediatR;

namespace CRM.Application.Notifications.Queries
{
    public class GetUnreadCountQuery : IRequest<UnreadCountDto>
    {
        public Guid RequestorUserId { get; set; }
    }
}

