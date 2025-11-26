using System.Threading.Tasks;
using CRM.Domain.Entities;

namespace CRM.Application.Quotations.Services
{
    public interface IQuotationPdfGenerationService
    {
        Task<byte[]> GenerateQuotationPdfAsync(Quotation quotation);
    }
}


