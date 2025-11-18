using System;

namespace CRM.Application.Clients.Commands
{
    public class RestoreClientCommand
    {
        public Guid ClientId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string MetadataJson { get; set; } = "{}";
    }
}

