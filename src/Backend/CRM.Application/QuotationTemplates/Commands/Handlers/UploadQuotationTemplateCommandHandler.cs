using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Services;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Application.QuotationTemplates.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Commands.Handlers
{
    public class UploadQuotationTemplateCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<UploadQuotationTemplateCommandHandler> _logger;

        // Allowed file types for templates
        private static readonly string[] AllowedMimeTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "text/html",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        private static readonly string[] AllowedExtensions = new[]
        {
            ".pdf", ".doc", ".docx", ".html", ".htm", ".xls", ".xlsx"
        };

        public UploadQuotationTemplateCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            IFileStorageService fileStorage,
            ILogger<UploadQuotationTemplateCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        public async Task<QuotationTemplateDto> Handle(UploadQuotationTemplateCommand command)
        {
            _logger.LogInformation("Uploading quotation template '{Name}' by user {UserId}", 
                command.Request.Name, command.CreatedByUserId);

            // Validate file
            if (command.FileStream == null || command.FileSize == 0)
            {
                throw new ArgumentException("File is required");
            }

            // Validate file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (command.FileSize > maxFileSize)
            {
                throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");
            }

            // Validate file type
            var contentType = command.ContentType;
            var fileName = command.FileName;
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!AllowedMimeTypes.Contains(contentType) && !AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    $"File type not allowed. Allowed types: PDF, Word (.doc, .docx), Excel (.xls, .xlsx), HTML");
            }

            // Validate visibility
            if (!Enum.TryParse<TemplateVisibility>(command.Request.Visibility, true, out var visibility))
            {
                throw new InvalidTemplateVisibilityException(command.Request.Visibility);
            }

            // Validate template type
            if (command.Request.TemplateType != "Quotation" && command.Request.TemplateType != "ProFormaInvoice")
            {
                throw new ArgumentException("TemplateType must be either 'Quotation' or 'ProFormaInvoice'");
            }

            // Check name uniqueness per owner
            var existingTemplate = await _db.QuotationTemplates
                .FirstOrDefaultAsync(t => 
                    t.Name == command.Request.Name && 
                    t.OwnerUserId == command.CreatedByUserId &&
                    t.DeletedAt == null);

            if (existingTemplate != null)
            {
                throw new InvalidOperationException($"A template with the name '{command.Request.Name}' already exists for this user.");
            }

            // Get user role
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == command.CreatedByUserId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {command.CreatedByUserId} not found.");
            }

            var ownerRole = user.Role?.RoleName ?? "SalesRep";

            // Upload file
            string fileUrl;
            try
            {
                fileUrl = await _fileStorage.UploadFileAsync(
                    command.FileStream,
                    fileName,
                    contentType,
                    "templates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload template file");
                throw new InvalidOperationException("Failed to upload file", ex);
            }

            // Create template entity
            var template = new QuotationTemplate
            {
                TemplateId = Guid.NewGuid(),
                Name = command.Request.Name,
                Description = command.Request.Description,
                OwnerUserId = command.CreatedByUserId,
                OwnerRole = ownerRole,
                Visibility = visibility,
                IsApproved = false,
                Version = 1,
                UsageCount = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                // File-based template properties
                IsFileBased = true,
                TemplateType = command.Request.TemplateType,
                FileName = fileName,
                FileUrl = fileUrl,
                FileSize = command.FileSize,
                MimeType = contentType
            };

            _db.QuotationTemplates.Add(template);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Template uploaded successfully. TemplateId: {TemplateId}", template.TemplateId);

            return _mapper.Map<QuotationTemplateDto>(template);
        }
    }
}

