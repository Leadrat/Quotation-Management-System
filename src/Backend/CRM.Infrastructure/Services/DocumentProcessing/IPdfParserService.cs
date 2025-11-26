namespace CRM.Infrastructure.Services.DocumentProcessing
{
    public interface IPdfParserService
    {
        /// <summary>
        /// Extracts text content from a PDF file
        /// </summary>
        Task<string> ExtractTextAsync(string pdfPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts text with position and formatting information
        /// </summary>
        Task<List<TextElement>> ExtractTextWithFormattingAsync(string pdfPath, CancellationToken cancellationToken = default);
    }

    public class TextElement
    {
        public string Text { get; set; } = string.Empty;
        public int Position { get; set; }
        public string? FontName { get; set; }
        public double? FontSize { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
    }
}

