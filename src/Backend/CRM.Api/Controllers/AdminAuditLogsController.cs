using CRM.Application.Admin.Queries;
using CRM.Application.Admin.Queries.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly GetAuditLogsQueryHandler _getLogsHandler;
    private readonly GetAuditLogByIdQueryHandler _getByIdHandler;
    private readonly ExportAuditLogsQueryHandler _exportHandler;

    public AdminAuditLogsController(
        GetAuditLogsQueryHandler getLogsHandler,
        GetAuditLogByIdQueryHandler getByIdHandler,
        ExportAuditLogsQueryHandler exportHandler)
    {
        _getLogsHandler = getLogsHandler;
        _getByIdHandler = getByIdHandler;
        _exportHandler = exportHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? performedBy = null,
        [FromQuery] string? actionType = null,
        [FromQuery] string? entity = null,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetAuditLogsQuery
        {
            PerformedBy = performedBy,
            ActionType = actionType,
            Entity = entity,
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _getLogsHandler.Handle(query);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLogById(Guid id)
    {
        var query = new GetAuditLogByIdQuery { Id = id };
        var result = await _getByIdHandler.Handle(query);

        if (result == null)
        {
            return NotFound(new { success = false, message = "Audit log entry not found" });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] Guid? performedBy = null,
        [FromQuery] string? actionType = null,
        [FromQuery] string? entity = null,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null)
    {
        var query = new ExportAuditLogsQuery
        {
            PerformedBy = performedBy,
            ActionType = actionType,
            Entity = entity,
            StartDate = startDate,
            EndDate = endDate
        };

        var csvBytes = await _exportHandler.Handle(query);
        var fileName = $"audit-logs-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(csvBytes, "text/csv", fileName);
    }
}

