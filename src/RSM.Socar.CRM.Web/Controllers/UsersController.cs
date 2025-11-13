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
using RSM.Socar.CRM.Web.OData;
using System.Text;

namespace RSM.Socar.CRM.Web.Controllers;

[Authorize]
[ODataRouteComponent("odata/[controller]")]
public sealed class UsersController : ODataController
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public UsersController(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    // ---------------------------------------------------
    // READ ALL
    // ---------------------------------------------------
    [EnableQuery(PageSize = 50)]
    [HttpGet("odata/Users")]
    public IQueryable<User> Get() =>
        _db.Users.AsNoTracking();

    // ---------------------------------------------------
    // READ SINGLE
    // ---------------------------------------------------
    [EnableQuery]
    [HttpGet("odata/Users({key})")]
    public async Task<ActionResult<User>> Get(int key, CancellationToken ct)
    {
        var entity = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == key, ct);

        return entity is null ? NotFound() : Ok(entity);
    }

    // ---------------------------------------------------
    // COUNT
    // ---------------------------------------------------
    [HttpGet("odata/Users/$count")]
    public IActionResult Count() =>
        Ok(_db.Users.Count());

    // ---------------------------------------------------
    // CREATE
    // ---------------------------------------------------
    [HttpPost("odata/Users")]
    public async Task<IActionResult> Post(
        [FromBody] CreateUserCommand.Request cmd,
        CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return Created(id!);
    }

    // ---------------------------------------------------
    // UPDATE (PUT) — concurrency protected
    // ---------------------------------------------------
    [HttpPut("odata/Users({key})")]
    public async Task<IActionResult> Put(
        [FromRoute] int key,
        [FromHeader(Name = "If-Match")] string? eTag,
        [FromBody] UpdateUserCommand.Request cmd,
        CancellationToken ct)
    {
        if (key != cmd.Id)
            return BadRequest("Key mismatch.");

        if (!ETagHelper.TryParse(eTag, out var rowVersion))
            return BadRequest("Invalid ETag");

        cmd = cmd with { RowVersion = rowVersion };
        await _mediator.Send(cmd, ct);

        return NoContent();
    }

    // ---------------------------------------------------
    // PATCH — Delta<T> + concurrency
    // ---------------------------------------------------
    [HttpPatch("odata/Users({key})")]
    public async Task<IActionResult> Patch(
        [FromRoute] int key,
        [FromHeader(Name = "If-Match")] string? eTag,
        [FromBody] Delta<User> delta,
        CancellationToken ct)
    {
        if (!ETagHelper.TryParse(eTag, out var rowVersion))
            return BadRequest("Invalid ETag");


        var cmd = new PatchUserCommand.Request(key, delta, rowVersion);
        await _mediator.Send(cmd, ct);

        return NoContent();
    }

    // ---------------------------------------------------
    // DELETE — triggers soft delete interceptor
    // ---------------------------------------------------
    [HttpDelete("odata/Users({key})")]
    public async Task<IActionResult> Delete(int key, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUserCommand.Request(key), ct);
        return NoContent();
    }


    // ---------------------------------------------------
    // ADMIN RESET PASSWORD (no concurrency required)
    // ---------------------------------------------------
    public sealed record ResetPasswordDto(string Password);

    [HttpPost("odata/Users({key})/SetPassword")]
    public async Task<IActionResult> SetPassword(
        [FromRoute] int key,
        [FromBody] ResetPasswordDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Password is required.");

        var cmd = new ResetPasswordCommand.Request(key, dto.Password);
        await _mediator.Send(cmd, ct);

        return Ok(true);
    }

    // ---------------------------------------------------
    // USER SELF CHANGE PASSWORD (requires concurrency)
    // ---------------------------------------------------
    public sealed record ChangePasswordDto(string OldPassword, string NewPassword);

    [HttpPost("odata/Users({key})/ChangePassword")]
    public async Task<IActionResult> ChangePassword(
        [FromRoute] int key,
        [FromHeader(Name = "If-Match")] string? eTag,
        [FromBody] ChangePasswordDto dto,
        CancellationToken ct)
    {
        if (!ETagHelper.TryParse(eTag, out var rowVersion))
            return BadRequest("Invalid ETag");


        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest("New password is required.");

        var cmd = new ChangePasswordCommand.Request(
            key,
            dto.OldPassword,
            dto.NewPassword,
            rowVersion
        );

        await _mediator.Send(cmd, ct);
        return Ok(true);
    }
}
