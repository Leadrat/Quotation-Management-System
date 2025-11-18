using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.Application.Reports.Dtos;

namespace CRM.Application.Reports.Services
{
    public class CsvExportService : IReportExportService
    {
        public Task<byte[]> ExportToPdfAsync(ReportData reportData)
        {
            throw new NotImplementedException("Use PdfExportService for PDF exports");
        }

        public Task<byte[]> ExportToExcelAsync(ReportData reportData)
        {
            throw new NotImplementedException("Use ExcelExportService for Excel exports");
        }

        public async Task<byte[]> ExportToCsvAsync(ReportData reportData)
        {
            var csv = new StringBuilder();

            // Add header
            csv.AppendLine($"Report: {reportData.Title}");
            csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine();

            // Add metrics
            if (reportData.Metrics.Any())
            {
                csv.AppendLine("Metrics:");
                foreach (var metric in reportData.Metrics)
                {
                    csv.AppendLine($"{metric.Name},{metric.Value}");
                }
                csv.AppendLine();
            }

            // Add details table
            if (reportData.Details.Any())
            {
                // Get headers from first row
                var headers = reportData.Details.First().Keys.ToList();
                csv.AppendLine(string.Join(",", headers));

                // Add rows
                foreach (var row in reportData.Details)
                {
                    var values = headers.Select(h => EscapeCsvValue(row.ContainsKey(h) ? row[h]?.ToString() ?? "" : ""));
                    csv.AppendLine(string.Join(",", values));
                }
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Escape quotes and wrap in quotes if contains comma, newline, or quote
            if (value.Contains(",") || value.Contains("\n") || value.Contains("\"") || value.Contains("\r"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}

