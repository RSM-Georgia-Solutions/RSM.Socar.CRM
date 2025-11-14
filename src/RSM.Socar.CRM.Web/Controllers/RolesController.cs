using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Application.Roles.Commands;

namespace RSM.Socar.CRM.Web.Controllers;

[Authorize]
[ODataRouteComponent("odata/[controller]")]
public sealed class RolesController : ODataController
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public RolesController(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    // ---------------------------------------------------
    // READ ALL
    // ---------------------------------------------------
    [EnableQuery(PageSize = 200)]
    [HttpGet("odata/Roles")]
    public IQueryable<Role> Get() =>
        _db.Roles.AsNoTracking();

    // ---------------------------------------------------
    // READ SINGLE
    // ---------------------------------------------------
    [EnableQuery]
    [HttpGet("odata/Roles({key})")]
    public async Task<ActionResult<Role>> Get(int key, CancellationToken ct)
    {
        var entity = await _db.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == key, ct);

        return entity is null ? NotFound() : Ok(entity);
    }

    // ---------------------------------------------------
    // COUNT
    // ---------------------------------------------------
    [HttpGet("odata/Roles/$count")]
    public IActionResult Count() =>
        Ok(_db.Roles.Count());

    // ---------------------------------------------------
    // CREATE
    // ---------------------------------------------------
    [HttpPost("odata/Roles")]
    public async Task<IActionResult> Post(
        [FromBody] CreateRoleCommand.Request cmd,
        CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return Created(id!);
    }

    // ---------------------------------------------------
    // UPDATE
    // ---------------------------------------------------
    [HttpPut("odata/Roles({key})")]
    public async Task<IActionResult> Put(
        int key,
        [FromBody] UpdateRoleCommand.Request cmd,
        CancellationToken ct)
    {
        cmd = cmd with { Id = key };
        await _mediator.Send(cmd, ct);

        return NoContent();
    }

    // ---------------------------------------------------
    // DELETE (soft-delete)
    // ---------------------------------------------------
    [HttpDelete("odata/Roles({key})")]
    public async Task<IActionResult> Delete(int key, CancellationToken ct)
    {
        await _mediator.Send(new DeleteRoleCommand.Request(key), ct);
        return NoContent();
    }
}
