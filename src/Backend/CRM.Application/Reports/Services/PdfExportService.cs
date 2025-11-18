using System;
using System.Threading.Tasks;
using CRM.Application.Reports.Dtos;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Reports.Services
{
    public class PdfExportService : IReportExportService
    {
        private readonly ILogger<PdfExportService> _logger;

        public PdfExportService(ILogger<PdfExportService> logger)
        {
            _logger = logger;
        }

        public Task<byte[]> ExportToPdfAsync(ReportData reportData)
        {
            // TODO: Implement PDF generation using QuestPDF or similar library
            // For now, return empty byte array
            _logger.LogWarning("PDF export not yet implemented. Install QuestPDF package.");
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<byte[]> ExportToExcelAsync(ReportData reportData)
        {
            throw new NotImplementedException("Use ExcelExportService for Excel exports");
        }

        public Task<byte[]> ExportToCsvAsync(ReportData reportData)
        {
            throw new NotImplementedException("Use CsvExportService for CSV exports");
        }
    }
}

