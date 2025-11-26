using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;
using CRM.Infrastructure.Services.DocumentProcessing;

namespace CRM.Infrastructure.Services.DocumentProcessing
{
    public class WordDocumentService : IWordDocumentService
    {
        public async Task<WordDocument> OpenDocumentAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var document = WordprocessingDocument.Open(filePath, true);
                    return new WordDocument
                    {
                        FilePath = filePath,
                        DocumentObject = document
                    };
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to open Word document: {ex.Message}", ex);
                }
            }, cancellationToken);
        }

        public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var document = WordprocessingDocument.Open(filePath, false);
                    var body = document.MainDocumentPart?.Document?.Body;
                    if (body == null) return string.Empty;

                    return body.InnerText;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to extract text from Word document: {ex.Message}", ex);
                }
            }, cancellationToken);
        }

        public async Task ReplaceTextWithPlaceholderAsync(string filePath, string originalText, string placeholder, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var document = WordprocessingDocument.Open(filePath, true);
                    var body = document.MainDocumentPart?.Document?.Body;
                    if (body == null) return;

                    // Find and replace text in all paragraphs while preserving formatting
                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        var textElements = paragraph.Descendants<Text>().ToList();
                        var fullText = string.Join("", textElements.Select(t => t.Text));

                        if (fullText.Contains(originalText))
                        {
                            // Replace text while preserving Run properties
                            var runs = paragraph.Descendants<Run>().ToList();
                            foreach (var run in runs)
                            {
                                var text = run.Descendants<Text>().FirstOrDefault();
                                if (text != null && text.Text.Contains(originalText))
                                {
                                    text.Text = text.Text.Replace(originalText, placeholder);
                                }
                            }
                        }
                    }

                    document.Save();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to replace text in Word document: {ex.Message}", ex);
                }
            }, cancellationToken);
        }

        public async Task SaveDocumentAsync(WordDocument document, string outputPath, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (document.DocumentObject is WordprocessingDocument wordDoc)
                    {
                        // Save the current document
                        wordDoc.Save();
                        wordDoc.Dispose();
                        
                        // Copy the file to the new location
                        if (document.FilePath != outputPath)
                        {
                            File.Copy(document.FilePath, outputPath, true);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid document object");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to save Word document: {ex.Message}", ex);
                }
            }, cancellationToken);
        }
    }
}

