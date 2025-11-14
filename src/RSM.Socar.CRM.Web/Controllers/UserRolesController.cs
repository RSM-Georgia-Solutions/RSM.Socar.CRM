using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.Roles.Commands;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RSM.Socar.CRM.Web.Controllers;

[Authorize]
[ODataRouteComponent("odata/[controller]")]
public sealed class UserRolesController : ODataController
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public UserRolesController(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    // ---------------------------------------------------
    // READ ALL USER-ROLE LINKS
    // ---------------------------------------------------
    [EnableQuery(PageSize = 200)]
    [HttpGet("odata/UserRoles")]
    public IQueryable<UserRole> Get() =>
        _db.UserRoles.AsNoTracking();

    // ---------------------------------------------------
    // READ SINGLE LINK
    // ---------------------------------------------------
    [EnableQuery]
    [HttpGet("odata/UserRoles({userId},{roleId})")]
    public async Task<ActionResult<UserRole>> Get(int userId, int roleId, CancellationToken ct)
    {
        var entity = await _db.UserRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, ct);

        return entity is null ? NotFound() : Ok(entity);
    }

    // ---------------------------------------------------
    // ASSIGN ROLE TO USER
    // ---------------------------------------------------
    public sealed record AssignRoleDto(int RoleId);

    [HttpPost("odata/Users({key})/AssignRole")]
    public async Task<IActionResult> Assign(
        int key,
        [FromBody] AssignRoleDto dto,
        CancellationToken ct)
    {
        var cmd = new AssignRoleToUserCommand.Request(key, dto.RoleId);
        await _mediator.Send(cmd, ct);

        return Ok(true);
    }

    // ---------------------------------------------------
    // REMOVE ROLE FROM USER
    // ---------------------------------------------------
    [HttpDelete("odata/Users({userId})/Roles({roleId})")]
    public async Task<IActionResult> Remove(
        int userId,
        int roleId,
        CancellationToken ct)
    {
        await _mediator.Send(new RemoveRoleFromUserCommand.Request(userId, roleId), ct);
        return NoContent();
    }
}
