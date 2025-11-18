using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Refunds.Dtos
{
    public class ReverseRefundRequest
    {
        [Required]
        [MaxLength(500)]
        public string ReversedReason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Comments { get; set; }
    }
}

