using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CRM.Domain.Entities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using CRM.Application.DocumentTemplates.Services;

namespace CRM.Infrastructure.Services
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        // Regex to find placeholders like {{CompanyName}}
        private static readonly Regex PlaceholderRegex = new Regex(@"\{\{([a-zA-Z0-9_]+)\}\}", RegexOptions.Compiled);

        public async Task<string> ProcessDocumentAsync(string filePath, string mimeType, CancellationToken cancellationToken = default)
        {
            // Simple extraction that returns document text based on mime type
            if (mimeType?.Contains("word") == true || filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                return await Task.Run(() =>
                {
                    using var wordDoc = WordprocessingDocument.Open(filePath, false);
                    return wordDoc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
                }, cancellationToken);
            }
            
            // For other types, return empty or throw
            throw new NotSupportedException($"Document type {mimeType} is not supported");
        }

        public async Task<List<TemplatePlaceholder>> AnalyzeDocumentAsync(string filePath, Guid templateId)
        {
            var placeholders = new List<TemplatePlaceholder>();
            var uniqueNames = new HashSet<string>();

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Template file not found", filePath);

            // OpenXML SDK usage to read Word document
            // Note: This runs synchronously as OpenXML SDK doesn't have async methods for DOM traversal
            await Task.Run(() =>
            {
                using (var wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart?.Document.Body;
                    if (body == null) return;

                    var text = body.InnerText; // Simple extraction, but we should iterate elements for better context
                    
                    // Better approach: Iterate all text elements
                    foreach (var textElement in body.Descendants<Text>())
                    {
                        var matches = PlaceholderRegex.Matches(textElement.Text);
                        foreach (Match match in matches)
                        {
                            var name = match.Groups[0].Value; // {{Name}}
                            if (uniqueNames.Contains(name)) continue;

                            uniqueNames.Add(name);
                            placeholders.Add(new TemplatePlaceholder
                            {
                                TemplateId = templateId,
                                PlaceholderName = name,
                                PlaceholderType = InferType(name),
                                OriginalText = name,
                                CreatedAt = DateTimeOffset.UtcNow,
                                UpdatedAt = DateTimeOffset.UtcNow
                            });
                        }
                    }
                }
            });

            return placeholders;
        }

        public async Task<byte[]> GenerateDocumentAsync(string templatePath, Dictionary<string, string> replacements)
        {
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Template file not found", templatePath);

            var memoryStream = new MemoryStream();
            
            // Copy template to memory stream
            using (var fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            
            memoryStream.Position = 0;

            using (var wordDoc = WordprocessingDocument.Open(memoryStream, true))
            {
                var body = wordDoc.MainDocumentPart?.Document.Body;
                if (body != null)
                {
                    foreach (var text in body.Descendants<Text>())
                    {
                        foreach (var kvp in replacements)
                        {
                            if (text.Text.Contains(kvp.Key))
                            {
                                text.Text = text.Text.Replace(kvp.Key, kvp.Value);
                            }
                        }
                    }
                    
                    // Save changes to the memory stream
                    wordDoc.MainDocumentPart.Document.Save();
                }
            }

            return memoryStream.ToArray();
        }

        private string InferType(string name)
        {
            var lower = name.ToLower();
            if (lower.Contains("customer") || lower.Contains("client")) return "Customer";
            if (lower.Contains("company") || lower.Contains("bank")) return "Company";
            if (lower.Contains("quotation") || lower.Contains("date") || lower.Contains("total")) return "Quotation";
            return "Other";
        }
    }
}
