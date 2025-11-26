using System.Threading.Tasks;
using CRM.Domain.Entities;

namespace CRM.Application.QuotationTemplates.Services
{
    /// <summary>
    /// Service for processing file-based templates by replacing placeholders with actual quotation data
    /// </summary>
    public interface ITemplateProcessingService
    {
        /// <summary>
        /// Processes a template file by replacing placeholders with quotation data and generates a PDF
        /// </summary>
        /// <param name="template">The template to process</param>
        /// <param name="quotation">The quotation with data to fill in the template</param>
        /// <returns>PDF bytes of the processed template</returns>
        Task<byte[]> ProcessTemplateToPdfAsync(QuotationTemplate template, Quotation quotation);
        
        /// <summary>
        /// Extracts text content from a template file for populating quotation fields
        /// </summary>
        /// <param name="template">The template to extract text from</param>
        /// <returns>Extracted text content from the template</returns>
        Task<string> ExtractTextFromTemplateAsync(QuotationTemplate template);
        
        /// <summary>
        /// Processes a template file by replacing placeholders with quotation data and generates HTML for web display
        /// </summary>
        /// <param name="template">The template to process</param>
        /// <param name="quotation">The quotation with data to fill in the template</param>
        /// <returns>HTML string of the processed template for web display</returns>
        Task<string> ProcessTemplateToHtmlAsync(QuotationTemplate template, Quotation quotation);

        /// <summary>
        /// Processes a Word (.docx) template and returns a populated DOCX document for download.
        /// Only applicable when the template is file-based and a Word document.
        /// </summary>
        /// <param name="template">The Word template to process</param>
        /// <param name="quotation">Quotation data for placeholder replacement</param>
        /// <returns>DOCX bytes</returns>
        Task<byte[]> ProcessTemplateToDocxAsync(QuotationTemplate template, Quotation quotation);

        /// <summary>
        /// Generates a DOCX document directly from quotation data (no template).
        /// </summary>
        /// <param name="quotation">Quotation to render</param>
        /// <returns>DOCX bytes</returns>
        Task<byte[]> GenerateQuotationDocxAsync(Quotation quotation);
    }
}
