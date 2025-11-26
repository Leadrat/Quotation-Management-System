using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Services;
using CRM.Application.DocumentTemplates.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DocumentTemplates.Commands.Handlers
{
    public class UploadDocumentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<UploadDocumentCommandHandler> _logger;

        public UploadDocumentCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            IFileStorageService fileStorage,
            ILogger<UploadDocumentCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        public async Task<DocumentTemplateDto> Handle(UploadDocumentCommand command)
        {
            _logger.LogInformation("Uploading document template '{Name}' by user {UserId}",
                command.Request.Name, command.CreatedByUserId);

            // Validate user exists
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == command.CreatedByUserId && u.DeletedAt == null);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {command.CreatedByUserId} not found.");
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

            // Upload file
            string fileUrl;
            try
            {
                if (command.FileStream.CanSeek)
                {
                    command.FileStream.Position = 0;
                }

                fileUrl = await _fileStorage.UploadFileAsync(
                    command.FileStream,
                    command.FileName,
                    command.ContentType,
                    "templates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {Message}", ex.Message);
                throw new InvalidOperationException($"File upload failed: {ex.Message}", ex);
            }

            // Create template record
            var now = DateTimeOffset.UtcNow;
            var template = new QuotationTemplate
            {
                TemplateId = Guid.NewGuid(),
                Name = command.Request.Name,
                Description = command.Request.Description,
                TemplateType = command.Request.TemplateType.ToString(),
                IsFileBased = true,
                FileName = command.FileName,
                OriginalFileName = command.FileName,
                FileUrl = fileUrl,
                FileSize = command.FileSize,
                MimeType = command.ContentType,
                ProcessingStatus = "Pending",
                OwnerUserId = command.CreatedByUserId,
                OwnerRole = user.Role?.RoleName ?? "SalesRep",
                Visibility = Domain.Enums.TemplateVisibility.Private,
                IsApproved = false,
                Version = 1,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.QuotationTemplates.Add(template);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Document template uploaded successfully. TemplateId: {TemplateId}", template.TemplateId);

            return _mapper.Map<DocumentTemplateDto>(template);
        }
    }
}

