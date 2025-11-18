namespace CRM.Shared.Validation
{
    public static class ValidationRegex
    {
        public const string Name = @"^[a-zA-Z\s\-']{2,100}$";
        public const string E164Mobile = @"^\+[1-9]\d{1,14}$";
    }
}
