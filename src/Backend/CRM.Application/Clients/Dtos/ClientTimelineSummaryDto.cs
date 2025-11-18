using System;

namespace CRM.Application.Clients.Dtos
{
    public class ClientTimelineSummaryDto
    {
        public Guid ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public long TotalChangeCount { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public string? DeletionReason { get; set; }
        public DateTimeOffset? RestorationWindowExpiresAt { get; set; }
        public ClientHistoryEntryDto? LatestEntry { get; set; }
    }
}

