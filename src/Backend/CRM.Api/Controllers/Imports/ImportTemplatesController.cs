using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Imports.Dtos;
using CRM.Application.Imports.Services;
using CRM.Domain.Imports;
using CRM.Infrastructure.Services.Imports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers.Imports
{
    [ApiController]
    [Route("api/imports")]
    public class ImportTemplatesController : ControllerBase
    {
        private readonly IAppDbContext _db;
        private readonly ImportStorageService _storage;
        private readonly ParseService _parser;

        public ImportTemplatesController(IAppDbContext db, ImportStorageService storage, ParseService parser)
        {
            _db = db;
            _storage = storage;
            _parser = parser;
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)] // 20 MB guard
        public async Task<IActionResult> Create([FromForm] IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "File is required" });
            }

            var ext = Path.GetExtension(file.FileName).Trim('.').ToLowerInvariant();
            var sourceType = ext switch
            {
                "pdf" => "pdf",
                "docx" => "docx",
                "xlsx" => "xlsx",
                "xslt" => "xslt",
                "dotx" => "dotx",
                _ => "unknown"
            };

            if (sourceType == "unknown")
            {
                return BadRequest(new { error = "Unsupported file type" });
            }

            // Save the file
            var fileRef = await _storage.SaveSourceAsync(file);

            // Parse (use the uploaded stream to avoid re-open delays)
            using var stream = file.OpenReadStream();
            var parsed = await _parser.ParseAsync(sourceType, stream, ct);

            var session = new ImportSession
            {
                ImportSessionId = Guid.NewGuid(),
                SourceType = sourceType,
                SourceFileRef = fileRef,
                Status = "Parsed",
                SuggestedMappingsJson = parsed.SuggestedMappingsJson,
                CreatedBy = User?.Identity?.Name ?? "system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.ImportSessions.AddAsync(session, ct);
            await _db.SaveChangesAsync(ct);

            var dto = new ImportSessionDto
            {
                ImportSessionId = session.ImportSessionId,
                SourceType = session.SourceType,
                Status = session.Status,
                SuggestedMappingsJson = session.SuggestedMappingsJson,
                ConfirmedMappingsJson = session.ConfirmedMappingsJson
            };

            return Created($"/api/imports/{session.ImportSessionId}", dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var session = await _db.ImportSessions.FindAsync(new object?[] { id }, ct);
            if (session == null) return NotFound();
            var dto = new ImportSessionDto
            {
                ImportSessionId = session.ImportSessionId,
                SourceType = session.SourceType,
                Status = session.Status,
                SuggestedMappingsJson = session.SuggestedMappingsJson,
                ConfirmedMappingsJson = session.ConfirmedMappingsJson
            };
            return Ok(dto);
        }

        [HttpPost("{id}/generate")]
        public async Task<IActionResult> Generate(Guid id, CancellationToken ct)
        {
            var session = await _db.ImportSessions.FindAsync(new object?[] { id }, ct);
            if (session == null) return NotFound();
            if (string.IsNullOrWhiteSpace(session.ConfirmedMappingsJson))
            {
                return BadRequest(new { error = "Confirm mappings before generating" });
            }

            // Placeholder generation: create a simple .txt file as the "preview" artifact
            var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previews");
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);
            var outputPath = Path.Combine(outputFolder, $"preview-{id}.txt");
            await System.IO.File.WriteAllTextAsync(outputPath, session.ConfirmedMappingsJson!, ct);

            session.Status = "Generated";
            session.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Ok(new { previewPath = outputPath });
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> Preview(Guid id, CancellationToken ct)
        {
            var session = await _db.ImportSessions.FindAsync(new object?[] { id }, ct);
            if (session == null) return NotFound();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previews", $"preview-{id}.txt");
            if (!System.IO.File.Exists(path)) return NotFound(new { error = "Preview not generated" });
            var bytes = await System.IO.File.ReadAllBytesAsync(path, ct);
            return File(bytes, "text/plain", Path.GetFileName(path));
        }

        public record SaveTemplateBody(string Name, string Type);

        [HttpPost("{id}/save-template")]
        public async Task<IActionResult> SaveTemplate(Guid id, [FromBody] SaveTemplateBody body, CancellationToken ct)
        {
            var session = await _db.ImportSessions.FindAsync(new object?[] { id }, ct);
            if (session == null) return NotFound();
            if (session.Status != "Generated") return BadRequest(new { error = "Generate preview before saving template" });

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "previews", $"preview-{id}.txt");
            if (!System.IO.File.Exists(outputPath)) return BadRequest(new { error = "No generated artifact to save" });

            var template = new ImportedTemplate
            {
                ImportedTemplateId = Guid.NewGuid(),
                ImportSessionId = session.ImportSessionId,
                Name = string.IsNullOrWhiteSpace(body.Name) ? $"Imported Template {DateTime.UtcNow:yyyyMMddHHmmss}" : body.Name,
                Type = string.IsNullOrWhiteSpace(body.Type) ? "generic" : body.Type,
                ContentRef = outputPath,
                Version = 1,
                CreatedBy = User?.Identity?.Name ?? "system",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.ImportedTemplates.AddAsync(template, ct);
            session.Status = "Saved";
            session.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Created($"/api/templates/{template.ImportedTemplateId}", new { templateId = template.ImportedTemplateId });
        }
    }
}
