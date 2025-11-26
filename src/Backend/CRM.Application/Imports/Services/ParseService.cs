using System.Threading.Tasks;

namespace CRM.Application.Imports.Services;

public class ParseService
{
    public record ParseResult(string SourceType, string? SuggestedMappingsJson);

    public Task<ParseResult> ParseAsync(string sourceType, Stream fileStream, CancellationToken ct = default)
    {
        // Placeholder: integrate with existing DocumentProcessingService/PDF/XLSX parsers later
        // For MVP, return an empty mapping skeleton
        var skeleton = "{ \"company\":{}, \"customer\":{}, \"items\":[], \"totals\":{} }";
        return Task.FromResult(new ParseResult(sourceType, skeleton));
    }
}
