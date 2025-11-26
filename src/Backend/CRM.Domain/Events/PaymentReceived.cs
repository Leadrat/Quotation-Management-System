namespace CRM.Domain.Events;

public class PaymentReceived
{
    public Guid PaymentId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();

    public PaymentReceived(Guid paymentId, Guid userId, string userEmail, string userName, decimal amount, string currency, string paymentMethod, string transactionId, string? notes = null)
    {
        PaymentId = paymentId;
        UserId = userId;
        UserEmail = userEmail;
        UserName = userName;
        Amount = amount;
        Currency = currency;
        PaymentMethod = paymentMethod;
        TransactionId = transactionId;
        Notes = notes;
        ReceivedAt = DateTime.UtcNow;
    }
}