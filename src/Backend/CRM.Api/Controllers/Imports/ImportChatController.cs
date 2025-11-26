using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Imports.Dtos;
using CRM.Application.Imports.LLM;
using CRM.Application.Imports.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers.Imports
{
    [ApiController]
    [Route("api/imports/{id}")]
    public class ImportChatController : ControllerBase
    {
        private readonly IAppDbContext _db;
        private readonly GeminiClient _gemini;
        private readonly MappingService _mappingService;

        public ImportChatController(IAppDbContext db, GeminiClient gemini, MappingService mappingService)
        {
            _db = db;
            _gemini = gemini;
            _mappingService = mappingService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat(Guid id, [FromBody] ChatMessageRequest req, CancellationToken ct)
        {
            var session = await _db.ImportSessions.FindAsync(new object?[] { id }, ct);
            if (session == null) return NotFound();
            if (string.IsNullOrWhiteSpace(req.Message)) return BadRequest(new { error = "message is required" });

            var prompt = $"You are assisting with mapping fields for a document import. Current suggested mappings JSON (may be partial): {session.SuggestedMappingsJson}. User message: {req.Message}. Reply succinctly with guidance and proposed JSON patch for mappings if applicable.";
            var text = await _gemini.ChatAsync(prompt, ct);
            return Ok(new { reply = text });
        }

        [HttpPost("mappings")]
        public async Task<IActionResult> SaveMappings(Guid id, [FromBody] SaveMappingsRequest req, CancellationToken ct)
        {
            var session = await _db.ImportSessions.FindAsync(new object?[] { id }, ct);
            if (session == null) return NotFound();

            var json = JsonSerializer.Serialize(req.Mappings);
            using var doc = JsonDocument.Parse(json);
            if (!_mappingService.ValidateRequiredMappings(doc.RootElement, out var error))
            {
                return BadRequest(new { error });
            }

            session.ConfirmedMappingsJson = json;
            session.Status = "Mapped";
            session.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Ok(new { ok = true });
        }
    }
}
