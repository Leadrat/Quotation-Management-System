using System;

namespace CRM.Application.Clients.Commands
{
    public class DeleteSavedSearchCommand
    {
        public Guid SavedSearchId { get; set; }
        public Guid UserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
