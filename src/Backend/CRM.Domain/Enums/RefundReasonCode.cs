namespace CRM.Domain.Enums
{
    /// <summary>
    /// Standard reason codes for refunds
    /// </summary>
    public enum RefundReasonCode
    {
        /// <summary>
        /// Client requested refund
        /// </summary>
        CLIENT_REQUEST = 0,

        /// <summary>
        /// System or processing error
        /// </summary>
        ERROR = 1,

        /// <summary>
        /// Discount adjustment refund
        /// </summary>
        DISCOUNT_ADJUSTMENT = 2,

        /// <summary>
        /// Order/service cancellation
        /// </summary>
        CANCELLATION = 3,

        /// <summary>
        /// Duplicate payment refund
        /// </summary>
        DUPLICATE_PAYMENT = 4,

        /// <summary>
        /// Other reason (requires comments)
        /// </summary>
        OTHER = 5
    }
}

