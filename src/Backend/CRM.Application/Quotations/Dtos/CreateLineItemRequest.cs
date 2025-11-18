namespace CRM.Application.Quotations.Dtos
{
    public class CreateLineItemRequest
    {
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitRate { get; set; }
    }
}

