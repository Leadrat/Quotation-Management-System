namespace CRM.Shared.Config
{
    public class HistorySettings
    {
        public int RetentionYears { get; set; } = 7;
        public int RestoreWindowDays { get; set; } = 30;
        public int DefaultPageSize { get; set; } = 20;
        public int MaxPageSize { get; set; } = 100;
        public int ExportRowLimit { get; set; } = 5000;
    }
}

