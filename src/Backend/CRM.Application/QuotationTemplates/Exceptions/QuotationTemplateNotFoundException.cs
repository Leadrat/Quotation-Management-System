using System;

namespace CRM.Application.QuotationTemplates.Exceptions
{
    public class QuotationTemplateNotFoundException : Exception
    {
        public QuotationTemplateNotFoundException(Guid templateId)
            : base($"Quotation template with ID '{templateId}' was not found.")
        {
        }

        public QuotationTemplateNotFoundException(string message)
            : base(message)
        {
        }
    }
}

