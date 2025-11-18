namespace CRM.Domain.Enums
{
    /// <summary>
    /// Event types in refund timeline
    /// </summary>
    public enum RefundTimelineEventType
    {
        /// <summary>
        /// Refund request created
        /// </summary>
        REQUESTED = 0,

        /// <summary>
        /// Refund approved
        /// </summary>
        APPROVED = 1,

        /// <summary>
        /// Refund rejected
        /// </summary>
        REJECTED = 2,

        /// <summary>
        /// Refund processing started
        /// </summary>
        PROCESSING = 3,

        /// <summary>
        /// Refund completed successfully
        /// </summary>
        COMPLETED = 4,

        /// <summary>
        /// Refund processing failed
        /// </summary>
        FAILED = 5,

        /// <summary>
        /// Refund reversed
        /// </summary>
        REVERSED = 6
    }
}

