using AutoMapper;
using CRM.Application.Reports.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class ReportProfile : Profile
    {
        public ReportProfile()
        {
            // DashboardBookmark mappings - handled manually in handlers due to JSON complexity
            // CreateMap<DashboardBookmark, DashboardConfig>() - not needed, handled directly

            // ScheduledReport mappings - ReportConfig handled manually due to JSON complexity
            CreateMap<ScheduledReport, ScheduledReportDto>()
                .ForMember(dest => dest.ReportConfig, opt => opt.Ignore());

            // ExportedReport mappings
            CreateMap<ExportedReport, ExportedReportDto>()
                .ForMember(dest => dest.DownloadUrl, opt => opt.MapFrom(src => 
                    $"/api/v1/reports/exports/{src.ExportId}/download"));
        }
    }

    public class ScheduledReportDto
    {
        public Guid ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public System.Collections.Generic.Dictionary<string, object>? ReportConfig { get; set; }
        public string RecurrencePattern { get; set; } = string.Empty;
        public string EmailRecipients { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset? LastSentAt { get; set; }
        public DateTimeOffset NextScheduledAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}

