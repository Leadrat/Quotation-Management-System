using System.Collections.Generic;

namespace CRM.Shared.Config
{
    public class SuspiciousActivitySettings
    {
        public short InlineThreshold { get; set; } = 7;
        public int RapidChangeThresholdPerHour { get; set; } = 10;
        public string OddHoursStart { get; set; } = "22:00";
        public string OddHoursEnd { get; set; } = "05:00";
        public List<string> AllowedIpCidrs { get; set; } = new();
        public string BatchJobCron { get; set; } = "*/5 * * * *";
    }
}

