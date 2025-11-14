using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.UserPermissions.Commands;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;

namespace RSM.Socar.CRM.Web.Controllers;

[Authorize]
[ODataRouteComponent("odata/[controller]")]
public sealed class UserPermissionsController : ODataController
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public UserPermissionsController(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    // ---------------------------------------------------
    // READ ALL
    // ---------------------------------------------------
    [EnableQuery(PageSize = 200)]
    [HttpGet("odata/UserPermissions")]
    public IQueryable<UserPermission> Get() =>
        _db.UserPermissions.AsNoTracking();

    // ---------------------------------------------------
    // READ SINGLE
    // ---------------------------------------------------
    [EnableQuery]
    [HttpGet("odata/UserPermissions({userId},{permissionId})")]
    public async Task<ActionResult<UserPermission>> Get(int userId, int permissionId, CancellationToken ct)
    {
        var entity = await _db.UserPermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PermissionId == permissionId, ct);

        return entity is null ? NotFound() : Ok(entity);
    }

    // ---------------------------------------------------
    // ASSIGN PERMISSION
    // ---------------------------------------------------
    public sealed record AssignPermissionDto(int PermissionId);

    [HttpPost("odata/Users({key})/AssignPermission")]
    public async Task<IActionResult> Assign(
        int key,
        [FromBody] AssignPermissionDto dto,
        CancellationToken ct)
    {
        var cmd = new GrantPermissionToUserCommand.Request(key, dto.PermissionId);
        await _mediator.Send(cmd, ct);

        return Ok(true);
    }

    // ---------------------------------------------------
    // REMOVE PERMISSION
    // ---------------------------------------------------
    [HttpDelete("odata/Users({userId})/Permissions({permissionId})")]
    public async Task<IActionResult> Remove(
        int userId,
        int permissionId,
        CancellationToken ct)
    {
        await _mediator.Send(new RevokePermissionFromUserCommand.Request(userId, permissionId), ct);
        return NoContent();
    }
}
