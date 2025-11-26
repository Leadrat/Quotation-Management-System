using System;
using System.Threading.Tasks;
using CRM.Application.Templates.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/templates")]
    [Authorize] // Assuming we want auth
    public class TemplatesController : ControllerBase
    {
        private readonly UploadTemplateCommandHandler _uploadHandler;
        private readonly SaveTemplateCommandHandler _saveHandler;

        public TemplatesController(
            UploadTemplateCommandHandler uploadHandler,
            SaveTemplateCommandHandler saveHandler)
        {
            _uploadHandler = uploadHandler;
            _saveHandler = saveHandler;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadTemplate([FromForm] IFormFile file, [FromForm] string templateType)
        {
            try
            {
                // Get user ID from claims (mocking for now or assuming standard claim)
                // var userId = Guid.Parse(User.FindFirst("sub")?.Value); 
                var userId = Guid.NewGuid(); // Placeholder

                var command = new UploadTemplateCommand
                {
                    File = file,
                    TemplateType = templateType,
                    UploadedByUserId = userId
                };

                var result = await _uploadHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while processing the template.", details = ex.Message });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveTemplate([FromBody] SaveTemplateCommand command)
        {
            try
            {
                // var userId = Guid.Parse(User.FindFirst("sub")?.Value);
                var userId = Guid.NewGuid(); // Placeholder
                command.CreatedByUserId = userId;

                var templateId = await _saveHandler.Handle(command);
                return CreatedAtAction(nameof(GetTemplate), new { id = templateId }, new { success = true, data = new { templateId } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to save template.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetTemplate(Guid id)
        {
            // Placeholder for Get Query
            return Ok(new { success = true, message = "Template details would be here" });
        }
    }
}
