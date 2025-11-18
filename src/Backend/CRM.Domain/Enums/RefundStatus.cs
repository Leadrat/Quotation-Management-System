namespace CRM.Domain.Enums
{
    /// <summary>
    /// Status of a refund request
    /// </summary>
    public enum RefundStatus
    {
        /// <summary>
        /// Awaiting approval
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Approved, ready for processing
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Currently being processed by gateway
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Successfully refunded
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Processing failed
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Refund was reversed
        /// </summary>
        Reversed = 5
    }
}

