namespace CRM.Shared.Config;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "crm.system";
    public string Audience { get; set; } = "crm.api";
    public int AccessTokenExpiration { get; set; } = 3600; // seconds
    public int RefreshTokenExpiration { get; set; } = 2592000; // seconds (30d)
    public string? PreviousSecret { get; set; }
}
