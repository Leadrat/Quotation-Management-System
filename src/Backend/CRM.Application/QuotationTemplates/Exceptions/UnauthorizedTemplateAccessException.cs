using System;

namespace CRM.Application.QuotationTemplates.Exceptions
{
    public class UnauthorizedTemplateAccessException : Exception
    {
        public UnauthorizedTemplateAccessException(Guid templateId)
            : base($"You do not have permission to access template with ID '{templateId}'.")
        {
        }

        public UnauthorizedTemplateAccessException(string message)
            : base(message)
        {
        }
    }
}

