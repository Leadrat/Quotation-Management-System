using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Templates.Commands
{
    public class SaveTemplateCommand
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TemplateType { get; set; }
        public string FilePath { get; set; }
        public string OriginalFileName { get; set; }
        public long FileSizeBytes { get; set; }
        public Guid CreatedByUserId { get; set; }
        public List<TemplatePlaceholderDto> Placeholders { get; set; }
    }

    public class TemplatePlaceholderDto
    {
        public string PlaceholderName { get; set; }
        public string PlaceholderType { get; set; }
        public string DefaultValue { get; set; }
    }

    public class SaveTemplateCommandHandler
    {
        // Assuming IAppDbContext interface exists based on ARCHITECTURE.md
        // If not, we'll need to find the actual DbContext class/interface
        private readonly IAppDbContext _context; 

        public SaveTemplateCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(SaveTemplateCommand command)
        {
            var template = new DocumentTemplate
            {
                TemplateId = Guid.NewGuid(),
                Name = command.Name,
                Description = command.Description,
                TemplateType = command.TemplateType,
                FilePath = command.FilePath,
                OriginalFileName = command.OriginalFileName,
                FileSizeBytes = command.FileSizeBytes,
                CreatedByUserId = command.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            foreach (var p in command.Placeholders)
            {
                template.Placeholders.Add(new TemplatePlaceholder
                {
                    PlaceholderId = Guid.NewGuid(),
                    TemplateId = template.TemplateId,
                    PlaceholderName = p.PlaceholderName,
                    PlaceholderType = p.PlaceholderType,
                    DefaultValue = p.DefaultValue,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }

            _context.DocumentTemplates.Add(template);
            await _context.SaveChangesAsync();

            return template.TemplateId;
        }
    }
}
