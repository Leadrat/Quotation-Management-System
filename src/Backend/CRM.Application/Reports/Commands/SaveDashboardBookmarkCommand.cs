using System;
using CRM.Application.Reports.Dtos;

namespace CRM.Application.Reports.Commands
{
    public class SaveDashboardBookmarkCommand
    {
        public DashboardConfig DashboardConfig { get; set; } = null!;
        public string DashboardName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public Guid UserId { get; set; }
        public Guid? BookmarkId { get; set; } // For updates
    }
}

