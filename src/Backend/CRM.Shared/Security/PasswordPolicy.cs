namespace CRM.Shared.Security
{
    public static class PasswordPolicy
    {
        // Minimum 8, at least one upper, one lower, one digit, one special
        public const string StrengthPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";
    }
}
