namespace CRM.Domain.Enums
{
    /// <summary>
    /// Recurrence pattern for scheduled reports
    /// </summary>
    public enum RecurrencePattern
    {
        /// <summary>
        /// Report runs daily
        /// </summary>
        Daily = 0,

        /// <summary>
        /// Report runs weekly
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// Report runs monthly
        /// </summary>
        Monthly = 2
    }
}

