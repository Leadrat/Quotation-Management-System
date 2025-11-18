namespace CRM.Shared.Constants;

public static class ValidationConstants
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;
    public const int MinNameLength = 2;
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 255;
    public const int MaxMobileLength = 20;
    public const int MaxLoginAttempts = 5;

    public const string PasswordRegex = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*(),.?\":{}|<>])[a-zA-Z0-9!@#$%^&*(),.?\":{}|<>]{8,128}$";
    public const string MobileRegex = "^\\+[1-9]\\d{1,14}$";
    public const string PhoneCodeRegex = "^\\+\\d{1,3}$";
    public const string NameRegex = "^[a-zA-Z\\s\\-']{2,100}$";
}
