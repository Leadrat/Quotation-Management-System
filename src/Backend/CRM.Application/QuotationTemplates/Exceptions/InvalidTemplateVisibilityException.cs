namespace CRM.Application.QuotationTemplates.Exceptions
{
    public class InvalidTemplateVisibilityException : Exception
    {
        public InvalidTemplateVisibilityException(string visibility)
            : base($"Invalid template visibility: '{visibility}'. Must be one of: Public, Team, Private.")
        {
        }
    }
}

