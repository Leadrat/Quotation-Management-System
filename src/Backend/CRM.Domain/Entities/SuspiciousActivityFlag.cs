using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("SuspiciousActivityFlags")]
    public class SuspiciousActivityFlag
    {
        public Guid FlagId { get; set; }
        public Guid HistoryId { get; set; }
        public Guid ClientId { get; set; }
        public short Score { get; set; }
        public List<string> Reasons { get; set; } = new();
        public DateTimeOffset DetectedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public string Status { get; set; } = "OPEN";

        public ClientHistory History { get; set; } = null!;

        [Column(TypeName = "jsonb")]
        public string Metadata { get; set; } = "{}";
    }
}

