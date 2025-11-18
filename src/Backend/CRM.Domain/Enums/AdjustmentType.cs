namespace CRM.Domain.Enums
{
    /// <summary>
    /// Type of adjustment to quotation
    /// </summary>
    public enum AdjustmentType
    {
        /// <summary>
        /// Discount percentage/amount changed
        /// </summary>
        DISCOUNT_CHANGE = 0,

        /// <summary>
        /// Total amount correction
        /// </summary>
        AMOUNT_CORRECTION = 1,

        /// <summary>
        /// Tax amount correction
        /// </summary>
        TAX_CORRECTION = 2
    }
}

