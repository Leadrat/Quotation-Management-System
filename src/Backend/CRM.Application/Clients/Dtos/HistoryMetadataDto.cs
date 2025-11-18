namespace CRM.Application.Clients.Dtos
{
    public class HistoryMetadataDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool IsAutomation { get; set; }
        public string RequestId { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
    }
}

