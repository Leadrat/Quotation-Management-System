using System;
using System.Collections.Generic;

namespace CRM.Application.Clients.Dtos
{
    public class SuspiciousActivityDto
    {
        public Guid FlagId { get; set; }
        public Guid HistoryId { get; set; }
        public Guid ClientId { get; set; }
        public short Score { get; set; }
        public IReadOnlyList<string> Reasons { get; set; } = Array.Empty<string>();
        public string Status { get; set; } = "OPEN";
        public DateTimeOffset DetectedAt { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
        public HistoryMetadataDto Metadata { get; set; } = new();
        public ClientHistoryEntryDto? HistoryEntry { get; set; }
    }
}

