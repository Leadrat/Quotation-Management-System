using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("ClientHistories")]
    public class ClientHistory
    {
        public Guid HistoryId { get; set; }
        public Guid ClientId { get; set; }
        public Guid? ActorUserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public List<string> ChangedFields { get; set; } = new();

        [Column(TypeName = "jsonb")]
        public string? BeforeSnapshot { get; set; }

        [Column(TypeName = "jsonb")]
        public string? AfterSnapshot { get; set; }

        public string? Reason { get; set; }

        [Column(TypeName = "jsonb")]
        public string Metadata { get; set; } = "{}";

        public short SuspicionScore { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public Client Client { get; set; } = null!;
        public User? ActorUser { get; set; }
        public ICollection<SuspiciousActivityFlag> Flags { get; set; } = new List<SuspiciousActivityFlag>();
    }
}

