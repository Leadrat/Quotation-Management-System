using System;

namespace CRM.Application.Quotations.Commands
{
    public class MarkQuotationAsViewedCommand
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}

 
