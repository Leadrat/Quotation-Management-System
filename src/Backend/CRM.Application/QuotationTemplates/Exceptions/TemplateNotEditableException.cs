using System;

namespace CRM.Application.QuotationTemplates.Exceptions
{
    public class TemplateNotEditableException : Exception
    {
        public TemplateNotEditableException(Guid templateId)
            : base($"Template with ID '{templateId}' cannot be edited. It may be deleted or not approved.")
        {
        }

        public TemplateNotEditableException(string message)
            : base(message)
        {
        }
    }
}

