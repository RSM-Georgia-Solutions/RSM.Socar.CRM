using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers; // you already have this
using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;

[Authorize]
[ODataRouteComponent("odata/[controller]")]
public sealed class UsersController : ODataController
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public UsersController(AppDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // GET /odata/Users
    [EnableQuery(PageSize = 50)]
    [HttpGet("odata/Users")]
    public IQueryable<User> Get() =>
        _db.Users.AsNoTracking();

    //GET /odata/Users(1)
    [EnableQuery]
    [HttpGet("odata/Users({key})")]
    public async Task<ActionResult<User>> Get(int key, CancellationToken ct)
    {
        var entity = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == key, ct);
        return entity is null ? NotFound() : Ok(entity);
    }

    // ✅ CREATE: POST /odata/Users
    [HttpPost("odata/Users")]
    public async Task<IActionResult> Post([FromBody] User user, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(user.PersonalNo))
            return BadRequest("PersonalNo is required.");

        user.Id = 0;
        user.RegisteredAtUtc = DateTime.UtcNow;
        user.IsActive = true;

        // keep hash untouched here; set via SetPassword action
        user.PasswordHash = user.PasswordHash is null ? "" : user.PasswordHash;

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return Created(user);
    }


    // PATCH /odata/Users(1)
    // Delta<User> allows partial updates while OData still works on the entity type
    [HttpPatch("odata/Users({key})")]
    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<User> patch, CancellationToken ct)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == key, ct);
        if (entity is null) return NotFound();

        // Prevent changing PasswordHash via PATCH
        patch.TryGetPropertyValue(nameof(RSM.Socar.CRM.Domain.Identity.User.PasswordHash), out var _);
        patch.TrySetPropertyValue(nameof(RSM.Socar.CRM.Domain.Identity.User.PasswordHash), entity.PasswordHash);

        patch.Patch(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // PUT /odata/Users(1) (replace non-sensitive fields)
    [HttpPut("odata/Users({key})")]
    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] User incoming, CancellationToken ct)
    {
        if (key != incoming.Id) return BadRequest();
        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == key, ct);
        if (entity is null) return NotFound();

        // Copy over allowed fields (don’t touch PasswordHash)
        entity.PersonalNo = incoming.PersonalNo;
        entity.FirstName = incoming.FirstName;
        entity.LastName = incoming.LastName;
        entity.BirthDate = incoming.BirthDate;
        entity.Mobile = incoming.Mobile;
        entity.Email = incoming.Email;
        entity.Position = incoming.Position;
        entity.IsActive = incoming.IsActive;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // POST /odata/Users({key})/SetPassword
    // Body: { "password": "NewPassw0rd!" }
    [HttpPost("({key})/SetPassword")]
    public async Task<IActionResult> SetPassword([FromRoute] int key, ODataActionParameters parameters, CancellationToken ct)
    {
        if (!parameters.TryGetValue("password", out var pwObj) || pwObj is not string password || string.IsNullOrWhiteSpace(password))
            return BadRequest("Password is required.");

        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == key, ct);
        if (entity is null) return NotFound();

        entity.PasswordHash = _hasher.HashPassword(entity, password);
        await _db.SaveChangesAsync(ct);

        return Ok(true);
    }
}
