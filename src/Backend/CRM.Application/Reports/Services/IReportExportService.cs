using System.Threading.Tasks;
using CRM.Application.Reports.Dtos;

namespace CRM.Application.Reports.Services
{
    public interface IReportExportService
    {
        Task<byte[]> ExportToPdfAsync(ReportData reportData);
        Task<byte[]> ExportToExcelAsync(ReportData reportData);
        Task<byte[]> ExportToCsvAsync(ReportData reportData);
    }
}

