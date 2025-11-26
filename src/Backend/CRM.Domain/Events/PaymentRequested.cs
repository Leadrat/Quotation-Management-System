namespace CRM.Domain.Events;

public class PaymentRequested
{
    public Guid PaymentId { get; set; }
    public Guid UserId { get; set; }
    public Guid QuotationId { get; set; } // Add missing QuotationId
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime DueDate { get; set; } // Add missing DueDate
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();

    public PaymentRequested(Guid paymentId, Guid userId, Guid quotationId, string userEmail, string userName, decimal amount, string currency, string paymentMethod, DateTime dueDate, string? description = null)
    {
        PaymentId = paymentId;
        UserId = userId;
        QuotationId = quotationId;
        UserEmail = userEmail;
        UserName = userName;
        Amount = amount;
        Currency = currency;
        PaymentMethod = paymentMethod;
        DueDate = dueDate;
        Description = description;
        RequestedAt = DateTime.UtcNow;
    }
}