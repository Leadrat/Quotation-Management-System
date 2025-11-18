using System;
using System.Threading.Tasks;
using CRM.Application.Reports.Dtos;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Reports.Services
{
    public class ExcelExportService : IReportExportService
    {
        private readonly ILogger<ExcelExportService> _logger;

        public ExcelExportService(ILogger<ExcelExportService> logger)
        {
            _logger = logger;
        }

        public Task<byte[]> ExportToPdfAsync(ReportData reportData)
        {
            throw new NotImplementedException("Use PdfExportService for PDF exports");
        }

        public Task<byte[]> ExportToExcelAsync(ReportData reportData)
        {
            // TODO: Implement Excel generation using EPPlus or ClosedXML
            // For now, return empty byte array
            _logger.LogWarning("Excel export not yet implemented. Install EPPlus or ClosedXML package.");
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<byte[]> ExportToCsvAsync(ReportData reportData)
        {
            throw new NotImplementedException("Use CsvExportService for CSV exports");
        }
    }
}

