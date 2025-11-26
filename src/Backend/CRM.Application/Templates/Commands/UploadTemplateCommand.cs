using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks;
using CRM.Domain.Entities;
using CRM.Application.Reports.Services;
using Microsoft.AspNetCore.Http;
using CRM.Application.Common.Persistence;
using CRM.Application.DocumentTemplates.Services;

namespace CRM.Application.Templates.Commands
{
    public class UploadTemplateCommand
    {
        public IFormFile File { get; set; }
        public string TemplateType { get; set; } // "Quotation" or "ProformaInvoice"
        public Guid UploadedByUserId { get; set; }
    }

    public class UploadTemplateResult
    {
        public Guid TemplateId { get; set; }
        public string FilePath { get; set; }
        public List<TemplatePlaceholder> DetectedPlaceholders { get; set; }
    }

    public class UploadTemplateCommandHandler
    {
        private readonly IFileStorageService _fileStorage;
        private readonly IDocumentProcessingService _documentProcessing;

        public UploadTemplateCommandHandler(
            IFileStorageService fileStorage,
            IDocumentProcessingService documentProcessing)
        {
            _fileStorage = fileStorage;
            _documentProcessing = documentProcessing;
        }

        public async Task<UploadTemplateResult> Handle(UploadTemplateCommand command)
        {
            // 1. Validate file
            var ext = Path.GetExtension(command.File.FileName).ToLower();
            if (ext != ".docx")
            {
                throw new ArgumentException("Only .docx files are supported for templates.");
            }

            // 2. Save file
            var filePath = await _fileStorage.SaveFileAsync(command.File, "templates");

            // 3. Create temporary ID for tracking (not saving to DB yet, or maybe we should?)
            // Spec says: "Upload -> Analyze -> Review -> Save". 
            // So we might just return the analysis first.
            var tempId = Guid.NewGuid();

            // 4. Analyze document
            var placeholders = await _documentProcessing.AnalyzeDocumentAsync(filePath, tempId);

            return new UploadTemplateResult
            {
                TemplateId = tempId,
                FilePath = filePath,
                DetectedPlaceholders = placeholders
            };
        }
    }
}
