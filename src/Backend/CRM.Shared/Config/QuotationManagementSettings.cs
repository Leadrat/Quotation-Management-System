namespace CRM.Shared.Config
{
    public class QuotationManagementSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string EmailFromAddress { get; set; } = string.Empty;
        public string EmailFromName { get; set; } = string.Empty;
        public int AccessLinkValidDays { get; set; } = 30;
        public int AccessLinkExpirationDays { get; set; } = 90;
        public int PdfCacheHours { get; set; } = 24;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableSmsNotifications { get; set; } = false;
    }
}

