using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Refunds.Dtos
{
    public class RejectRefundRequest
    {
        [Required]
        [MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Comments { get; set; }
    }
}

