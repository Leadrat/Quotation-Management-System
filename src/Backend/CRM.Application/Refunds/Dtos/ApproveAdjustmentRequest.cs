using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Refunds.Dtos
{
    public class ApproveAdjustmentRequest
    {
        [MaxLength(1000)]
        public string? Comments { get; set; }
    }
}

