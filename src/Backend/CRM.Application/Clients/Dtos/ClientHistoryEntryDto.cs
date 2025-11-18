using System;
using System.Collections.Generic;

namespace CRM.Application.Clients.Dtos
{
    public class ClientHistoryEntryDto
    {
        public Guid HistoryId { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ActorUserId { get; set; }
        public string ActorDisplayName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public IReadOnlyList<string> ChangedFields { get; set; } = Array.Empty<string>();
        public object? BeforeSnapshot { get; set; }
        public object? AfterSnapshot { get; set; }
        public string? Reason { get; set; }
        public HistoryMetadataDto Metadata { get; set; } = new();
        public short SuspicionScore { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}

