using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.Users.Commands;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;

[Authorize]
[ODataRouteComponent("odata/[controller]")]
public sealed class UsersController : ODataController
{
    private readonly AppDbContext _db;   // read side for OData
    private readonly IMediator _mediator; // write side via Application

    public UsersController(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    // READS stay OData (DB-pushdown)
    [EnableQuery(PageSize = 50)]
    [HttpGet("odata/Users")]
    public IQueryable<User> Get() => _db.Users.AsNoTracking();

    [EnableQuery]
    [HttpGet("odata/Users({key})")]
    public async Task<ActionResult<User>> Get(int key, CancellationToken ct)
    {
        var entity = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == key, ct);
        return entity is null ? NotFound() : Ok(entity);
    }

    // CREATE -> command
    [HttpPost("odata/Users")]
    public async Task<IActionResult> Post([FromBody] CreateUserCommand.Request cmd, CancellationToken ct)
    {
        var created = await _mediator.Send(cmd, ct);
        return Created(created!);
    }

    // UPDATE -> command (If-Match ETag should populate RowVersion on cmd)
    [HttpPut("odata/Users({key})")]
    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] UpdateUserCommand.Request cmd, CancellationToken ct)
    {
        if (key != cmd.Id) return BadRequest();
        await _mediator.Send(cmd, ct);
        return NoContent();
    }

    // PATCH -> command
    [HttpPatch("odata/Users({key})")]
    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<User> delta, CancellationToken ct)
    {
        await _mediator.Send(new PatchUserCommand.Request(key, delta), ct);
        return NoContent();
    }

    public sealed record SetPasswordDto(string Password);
    [HttpPost("odata/Users({key})/SetPassword")]
    public async Task<IActionResult> SetPassword([FromRoute] int key, [FromBody] SetPasswordDto body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body?.Password))
            return BadRequest("Password is required.");

        await _mediator.Send(new SetUserPasswordCommand.Request(key, body.Password), ct);
        return Ok(true);
    }
}
