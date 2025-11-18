using System;

namespace CRM.Domain.Events
{
    public class PaymentGatewayConfigUpdated
    {
        public Guid ConfigId { get; set; }
        public Guid? CompanyId { get; set; }
        public string GatewayName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public bool IsTestMode { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}

