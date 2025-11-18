using System;
using CRM.Application.Quotations.Dtos;

namespace CRM.Application.Quotations.Commands
{
    public class SubmitQuotationResponseCommand
    {
        public string AccessToken { get; set; } = string.Empty;
        public SubmitQuotationResponseRequest Request { get; set; } = null!;
        public string? IpAddress { get; set; }
    }
}

 