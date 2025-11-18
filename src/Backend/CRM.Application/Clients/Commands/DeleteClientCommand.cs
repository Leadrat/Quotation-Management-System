using System;

namespace CRM.Application.Clients.Commands
{
    public class DeleteClientCommand
    {
        public Guid ClientId { get; set; }
        public Guid DeletedByUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}
