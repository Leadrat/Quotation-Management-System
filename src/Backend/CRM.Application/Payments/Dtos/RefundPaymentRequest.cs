using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Payments.Dtos
{
    public class RefundPaymentRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than zero")]
        public decimal? Amount { get; set; } // Optional, defaults to full refund

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}

