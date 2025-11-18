using System.Collections.Generic;
using CRM.Application.Common.Results;

namespace CRM.Application.Notifications.Dtos
{
    public class PagedNotificationsResult : PagedResult<NotificationDto>
    {
        // Inherits from PagedResult<T> which has:
        // - bool Success
        // - T[] Data
        // - int PageNumber
        // - int PageSize
        // - int TotalCount
    }
}

