using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Services;
using CRM.Application.DocumentTemplates.Dtos;
using CRM.Application.DocumentTemplates.Services;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DocumentTemplates.Commands.Handlers
{
    public class ConvertDocumentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDocumentProcessingService _documentProcessingService;
        private readonly IPlaceholderIdentificationService _placeholderIdentificationService;
        private readonly ITemplateConversionService _templateConversionService;
        private readonly IFileStorageService _fileStorage;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConvertDocumentCommandHandler> _logger;

        public ConvertDocumentCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            IDocumentProcessingService documentProcessingService,
            IPlaceholderIdentificationService placeholderIdentificationService,
            ITemplateConversionService templateConversionService,
            IFileStorageService fileStorage,
            IConfiguration configuration,
            ILogger<ConvertDocumentCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _documentProcessingService = documentProcessingService;
            _placeholderIdentificationService = placeholderIdentificationService;
            _templateConversionService = templateConversionService;
            _fileStorage = fileStorage;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<DocumentTemplateDto> Handle(ConvertDocumentCommand command)
        {
            _logger.LogInformation("Converting document template {TemplateId} by user {UserId}",
                command.TemplateId, command.RequestedByUserId);

            // Get template
            var template = await _db.QuotationTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == command.TemplateId && t.DeletedAt == null);

            if (template == null)
            {
                throw new InvalidOperationException($"Template with ID {command.TemplateId} not found.");
            }

            if (!template.IsFileBased || string.IsNullOrEmpty(template.FileUrl))
            {
                throw new InvalidOperationException("Template is not file-based or file URL is missing.");
            }

            // Update status to Processing
            template.ProcessingStatus = "Processing";
            template.ProcessingErrorMessage = null;
            await _db.SaveChangesAsync();

            try
            {
                // Get file path from storage
                var filePath = template.FileUrl;
                if (!Path.IsPathRooted(filePath))
                {
                    // If relative path, construct full path
                    var basePath = _configuration["FileStorage:BasePath"] ?? "wwwroot";
                    filePath = Path.Combine(basePath, filePath);
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Template file not found at: {filePath}");
                }

                // Process document to extract text
                var documentText = await _documentProcessingService.ProcessDocumentAsync(
                    filePath,
                    template.MimeType ?? "application/octet-stream");

                // Identify placeholders
                var companyPlaceholders = await _placeholderIdentificationService.IdentifyCompanyDetailsAsync(documentText);
                var customerPlaceholders = await _placeholderIdentificationService.IdentifyCustomerDetailsAsync(documentText);
                var allPlaceholders = companyPlaceholders.Concat(customerPlaceholders).ToList();

                // Convert document to template
                var templatePath = _configuration["FileUpload:TemplatePath"] ?? "wwwroot/templates";
                var outputFileName = $"{template.TemplateId}_{template.Name.Replace(" ", "_")}.docx";
                var outputFilePath = Path.Combine(templatePath, outputFileName);

                // Ensure directory exists
                Directory.CreateDirectory(templatePath);

                await _templateConversionService.ConvertToTemplateAsync(
                    filePath,
                    outputFilePath,
                    allPlaceholders,
                    template.MimeType ?? "application/octet-stream");

                // Save placeholders to database
                var now = DateTimeOffset.UtcNow;
                var existingPlaceholders = await _db.TemplatePlaceholders
                    .Where(p => p.TemplateId == template.TemplateId)
                    .ToListAsync();

                if (existingPlaceholders.Any())
                {
                    _db.TemplatePlaceholders.RemoveRange(existingPlaceholders);
                }

                foreach (var placeholder in allPlaceholders)
                {
                    var placeholderEntity = new TemplatePlaceholder
                    {
                        PlaceholderId = Guid.NewGuid(),
                        TemplateId = template.TemplateId,
                        PlaceholderName = placeholder.PlaceholderName,
                        PlaceholderType = placeholder.PlaceholderType,
                        OriginalText = placeholder.OriginalText,
                        PositionInDocument = placeholder.PositionInDocument,
                        IsManuallyAdded = false,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _db.TemplatePlaceholders.Add(placeholderEntity);
                }

                // Update template
                template.TemplateFilePath = outputFilePath;
                template.ProcessingStatus = "Completed";
                template.UpdatedAt = now;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Document template converted successfully. TemplateId: {TemplateId}, Placeholders: {Count}",
                    template.TemplateId, allPlaceholders.Count);

                return _mapper.Map<DocumentTemplateDto>(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting document template {TemplateId}: {Message}",
                    template.TemplateId, ex.Message);

                template.ProcessingStatus = "Failed";
                template.ProcessingErrorMessage = ex.Message;
                await _db.SaveChangesAsync();

                throw;
            }
        }
    }
}

