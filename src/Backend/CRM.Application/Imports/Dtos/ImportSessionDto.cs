namespace CRM.Application.Imports.Dtos;

public class ImportSessionDto
{
    public Guid ImportSessionId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SuggestedMappingsJson { get; set; }
    public string? ConfirmedMappingsJson { get; set; }
}
