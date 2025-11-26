namespace CRM.Domain.Enums
{
    public enum BillingCycle
    {
        Monthly = 1,        // Monthly billing
        Quarterly = 2,      // Quarterly billing (3 months)
        HalfYearly = 3,    // Half-yearly billing (6 months)
        Yearly = 4,         // Yearly billing (12 months)
        MultiYear = 5       // Multi-year billing (2-5 years)
    }
}

