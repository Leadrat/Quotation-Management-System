using System;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Payments.Dtos
{
    public class InitiatePaymentRequest
    {
        [Required]
        public Guid QuotationId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentGateway { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal? Amount { get; set; } // Optional, defaults to quotation total

        [StringLength(3)]
        public string? Currency { get; set; } // Optional, defaults to quotation currency or INR
    }
}

