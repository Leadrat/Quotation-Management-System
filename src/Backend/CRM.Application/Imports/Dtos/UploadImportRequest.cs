namespace CRM.Application.Imports.Dtos;

public class UploadImportRequest
{
    public string SourceType { get; set; } = string.Empty; // pdf, docx, xlsx, xslt, dotx
}
