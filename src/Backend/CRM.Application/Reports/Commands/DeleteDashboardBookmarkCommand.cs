using System;

namespace CRM.Application.Reports.Commands
{
    public class DeleteDashboardBookmarkCommand
    {
        public Guid BookmarkId { get; set; }
        public Guid UserId { get; set; }
    }
}

