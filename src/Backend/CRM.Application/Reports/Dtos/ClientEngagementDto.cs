using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class ClientEngagementDto
    {
        public decimal ViewRate { get; set; }
        public decimal ResponseRate { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal AverageResponseTimeHours { get; set; }
        public List<ClientEngagementData> ClientEngagement { get; set; } = new();
        public List<ResponseTimeData> ResponseTimeByPeriod { get; set; } = new();
    }

    public class ClientEngagementData
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int QuotationsSent { get; set; }
        public int QuotationsViewed { get; set; }
        public int QuotationsResponded { get; set; }
        public int QuotationsAccepted { get; set; }
        public decimal ViewRate { get; set; }
        public decimal ResponseRate { get; set; }
        public decimal ConversionRate { get; set; }
    }

    public class ResponseTimeData
    {
        public string Period { get; set; } = string.Empty;
        public decimal AverageHours { get; set; }
    }
}

