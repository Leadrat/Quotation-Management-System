namespace CRM.Domain.Enums
{
    public enum ProductType
    {
        Subscription = 1,           // Subscription products (per user per month)
        AddOnSubscription = 2,      // Add-on services (subscription)
        AddOnOneTime = 3,           // Add-on services (one-time)
        CustomDevelopment = 4        // Custom development charges
    }
}

