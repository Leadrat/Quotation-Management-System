namespace CRM.Shared.Config
{
    public class QuotationSettings
    {
        public string NumberFormat { get; set; } = "QT-{Year}-{Sequence}";
        public int DefaultValidDays { get; set; } = 30;
        public decimal TaxRate { get; set; } = 18.0m;
        public int MaxLineItems { get; set; } = 100;
    }
}

