using System.ComponentModel.DataAnnotations;
using CRM.Domain.Enums;

namespace CRM.Application.Refunds.Dtos
{
    public class UpdateRefundRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
        public decimal? RefundAmount { get; set; }

        [MaxLength(500)]
        public string? RefundReason { get; set; }

        public RefundReasonCode? RefundReasonCode { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }
    }
}

