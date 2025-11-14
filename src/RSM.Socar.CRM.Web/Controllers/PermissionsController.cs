using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;

namespace RSM.Socar.CRM.Web.Controllers;

[Authorize]
[ODataRouteComponent("odata/[controller]")]
public sealed class PermissionsController : ODataController
{
    private readonly AppDbContext _db;

    public PermissionsController(AppDbContext db)
    {
        _db = db;
    }

    // ---------------------------------------------------
    // READ ALL
    // ---------------------------------------------------
    [EnableQuery(PageSize = 200)]
    [HttpGet("odata/Permissions")]
    public IQueryable<Permission> Get() =>
        _db.Permissions.AsNoTracking();

    // ---------------------------------------------------
    // READ SINGLE
    // ---------------------------------------------------
    [EnableQuery]
    [HttpGet("odata/Permissions({key})")]
    public async Task<ActionResult<Permission>> Get(int key, CancellationToken ct)
    {
        var entity = await _db.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == key, ct);

        return entity is null ? NotFound() : Ok(entity);
    }

    // ---------------------------------------------------
    // COUNT
    // ---------------------------------------------------
    [HttpGet("odata/Permissions/$count")]
    public IActionResult Count() =>
        Ok(_db.Permissions.Count());
}
