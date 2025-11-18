using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Roles.Queries;
using CRM.Application.Roles.Queries.Handlers;
using CRM.Application.Roles.Commands;
using CRM.Application.Roles.Commands.Handlers;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IAuditLogger _audit;

    public RolesController(AppDbContext db, IMapper mapper, IAuditLogger audit)
    {
        _db = db;
        _mapper = mapper;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var handler = new GetAllRolesQueryHandler(_db, _mapper);
        var result = await handler.Handle(new GetAllRolesQuery { IsActive = isActive, PageNumber = pageNumber, PageSize = pageSize });
        return Ok(result);
    }

    [HttpGet("{roleId}")]
    public async Task<IActionResult> GetById([FromRoute] Guid roleId)
    {
        var handler = new GetRoleByIdQueryHandler(_db);
        var result = await handler.Handle(new GetRoleByIdQuery { RoleId = roleId });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand cmd)
    {
        var handler = new CreateRoleCommandHandler(_db);
        var result = await handler.Handle(cmd);
        await _audit.LogAsync("role_create", new { result.RoleId, result.RoleName });
        return Created($"/api/v1/roles/{result.RoleId}", new { success = true, data = result });
    }

    [HttpPut("{roleId}")]
    public async Task<IActionResult> Update([FromRoute] Guid roleId, [FromBody] UpdateRoleCommand cmd)
    {
        cmd.RoleId = roleId;
        var handler = new UpdateRoleCommandHandler(_db);
        var result = await handler.Handle(cmd);
        await _audit.LogAsync("role_update", new { result.RoleId, result.RoleName });
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> Delete([FromRoute] Guid roleId)
    {
        var handler = new DeleteRoleCommandHandler(_db);
        await handler.Handle(new DeleteRoleCommand { RoleId = roleId });
        await _audit.LogAsync("role_delete", new { RoleId = roleId });
        return Ok(new { success = true, message = "Role deleted successfully" });
    }
}
