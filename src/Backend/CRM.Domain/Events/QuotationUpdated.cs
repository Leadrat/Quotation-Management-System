using System;
using System.Collections.Generic;

namespace CRM.Domain.Events
{
    public class QuotationUpdated
    {
        public Guid QuotationId { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Dictionary<string, object> Changes { get; set; } = new();
    }
}

