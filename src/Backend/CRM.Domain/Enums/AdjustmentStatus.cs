namespace CRM.Domain.Enums
{
    /// <summary>
    /// Status of an adjustment request
    /// </summary>
    public enum AdjustmentStatus
    {
        /// <summary>
        /// Awaiting approval
        /// </summary>
        PENDING = 0,

        /// <summary>
        /// Approved, ready to apply
        /// </summary>
        APPROVED = 1,

        /// <summary>
        /// Rejected by approver
        /// </summary>
        REJECTED = 2,

        /// <summary>
        /// Applied to quotation
        /// </summary>
        APPLIED = 3
    }
}

