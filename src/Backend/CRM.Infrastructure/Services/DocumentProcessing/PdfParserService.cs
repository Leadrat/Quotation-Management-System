using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using CRM.Infrastructure.Services.DocumentProcessing;

namespace CRM.Infrastructure.Services.DocumentProcessing
{
    public class PdfParserService : IPdfParserService
    {
        public async Task<string> ExtractTextAsync(string pdfPath, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
                    var text = new System.Text.StringBuilder();

                    for (int i = 0; i < document.PageCount; i++)
                    {
                        var page = document.Pages[i];
                        // PdfSharpCore doesn't have built-in text extraction
                        // For now, return placeholder - will need to use PdfPig or another library
                        text.AppendLine($"[Page {i + 1} - Text extraction requires PdfPig or similar library]");
                    }

                    return text.ToString();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to parse PDF: {ex.Message}", ex);
                }
            }, cancellationToken);
        }

        public async Task<List<TextElement>> ExtractTextWithFormattingAsync(string pdfPath, CancellationToken cancellationToken = default)
        {
            // PdfSharpCore doesn't support text extraction with formatting
            // This would require PdfPig or another library
            var text = await ExtractTextAsync(pdfPath, cancellationToken);
            return new List<TextElement>
            {
                new TextElement { Text = text, Position = 0 }
            };
        }
    }
}

