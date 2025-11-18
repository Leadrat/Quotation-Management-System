using System;
using System.IO;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Dtos;
using CRM.Application.Reports.Services;
using CRM.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Reports.Commands.Handlers
{
    public class ExportReportCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IReportExportService _exportService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ExportReportCommandHandler> _logger;

        public ExportReportCommandHandler(
            IAppDbContext db,
            IReportExportService exportService,
            IFileStorageService fileStorageService,
            ILogger<ExportReportCommandHandler> logger)
        {
            _db = db;
            _exportService = exportService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ExportedReportDto> Handle(ExportReportCommand command)
        {
            // Generate report data first (simplified - in real scenario, would fetch from cache or regenerate)
            var reportData = new ReportData
            {
                ReportType = command.Request.ReportId, // Using ReportId as ReportType for now
                Title = "Exported Report"
            };

            byte[] fileBytes;
            string fileExtension;

            // Export based on format
            switch (command.Request.Format.ToLowerInvariant())
            {
                case "pdf":
                    fileBytes = await _exportService.ExportToPdfAsync(reportData);
                    fileExtension = ".pdf";
                    break;
                case "excel":
                    fileBytes = await _exportService.ExportToExcelAsync(reportData);
                    fileExtension = ".xlsx";
                    break;
                case "csv":
                    fileBytes = await _exportService.ExportToCsvAsync(reportData);
                    fileExtension = ".csv";
                    break;
                default:
                    throw new ArgumentException($"Unsupported export format: {command.Request.Format}");
            }

            // Save file
            var fileName = $"report_{Guid.NewGuid()}{fileExtension}";
            var filePath = await _fileStorageService.SaveFileAsync(fileName, fileBytes);

            // Create ExportedReport record
            var exportedReport = new ExportedReport
            {
                ExportId = Guid.NewGuid(),
                CreatedByUserId = command.UserId,
                ReportType = command.Request.ReportId,
                ExportFormat = command.Request.Format.ToLowerInvariant(),
                FilePath = filePath,
                FileSize = fileBytes.Length,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.ExportedReports.Add(exportedReport);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Report exported: {ExportId}, Format: {Format}, Size: {Size} bytes",
                exportedReport.ExportId, exportedReport.ExportFormat, exportedReport.FileSize);

            return new ExportedReportDto
            {
                ExportId = exportedReport.ExportId,
                ReportType = exportedReport.ReportType,
                ExportFormat = exportedReport.ExportFormat,
                FilePath = exportedReport.FilePath,
                FileSize = exportedReport.FileSize,
                CreatedAt = exportedReport.CreatedAt,
                DownloadUrl = $"/api/v1/reports/exports/{exportedReport.ExportId}/download"
            };
        }
    }
}

