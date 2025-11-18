namespace CRM.Application.DiscountApprovals.Commands
{
    public class EscalateDiscountApprovalCommand
    {
        public Guid ApprovalId { get; set; }
        public Guid EscalatedByUserId { get; set; }
        public string? Reason { get; set; }
    }
}

