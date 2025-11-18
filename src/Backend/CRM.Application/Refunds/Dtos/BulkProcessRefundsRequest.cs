using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Refunds.Dtos
{
    public class BulkProcessRefundsRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one refund ID is required")]
        public List<Guid> RefundIds { get; set; } = new();

        [MaxLength(1000)]
        public string? Comments { get; set; }
    }
}

