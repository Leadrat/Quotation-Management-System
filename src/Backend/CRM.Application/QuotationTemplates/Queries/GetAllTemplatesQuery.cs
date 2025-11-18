namespace CRM.Application.QuotationTemplates.Queries
{
    public class GetAllTemplatesQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Visibility { get; set; } // "Public", "Team", "Private"
        public bool? IsApproved { get; set; }
        public Guid? OwnerUserId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

