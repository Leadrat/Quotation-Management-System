using CRM.Domain.Entities;

namespace CRM.Application.Quotations.Services
{
    public interface IQuotationPdfGenerationService
    {
        byte[] GenerateQuotationPdf(Quotation quotation);
    }
}


