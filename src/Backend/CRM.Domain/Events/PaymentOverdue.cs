namespace CRM.Domain.Events;

public class PaymentOverdue
{
    public Guid PaymentId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime OverdueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string? ReminderLevel { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();

    public PaymentOverdue(Guid paymentId, Guid userId, string userEmail, string userName, decimal amount, string currency, DateTime dueDate, string? reminderLevel = null)
    {
        PaymentId = paymentId;
        UserId = userId;
        UserEmail = userEmail;
        UserName = userName;
        Amount = amount;
        Currency = currency;
        DueDate = dueDate;
        OverdueDate = DateTime.UtcNow;
        DaysOverdue = (DateTime.UtcNow - dueDate).Days;
        ReminderLevel = reminderLevel;
    }
}