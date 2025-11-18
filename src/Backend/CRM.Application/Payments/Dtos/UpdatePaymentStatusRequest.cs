using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Payments.Dtos
{
    public class UpdatePaymentStatusRequest
    {
        [Required]
        public string PaymentReference { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty; // "success", "failed", "pending"

        public decimal? Amount { get; set; }

        public string? Currency { get; set; }

        public DateTimeOffset? PaymentDate { get; set; }

        public string? FailureReason { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }
    }
}

