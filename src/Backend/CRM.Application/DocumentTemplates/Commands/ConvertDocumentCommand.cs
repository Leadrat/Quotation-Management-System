namespace CRM.Application.DocumentTemplates.Commands
{
    public class ConvertDocumentCommand
    {
        public Guid TemplateId { get; set; }
        public Guid RequestedByUserId { get; set; }
    }
}

