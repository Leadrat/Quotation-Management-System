using System;

namespace CRM.Application.Quotations.Dtos
{
    public class LineItemDto
    {
        public Guid LineItemId { get; set; }
        public int SequenceNumber { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitRate { get; set; }
        public decimal Amount { get; set; }
    }
}

