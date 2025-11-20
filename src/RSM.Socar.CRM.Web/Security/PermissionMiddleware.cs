using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RSM.Socar.CRM.Infrastructure.Persistence;

namespace RSM.Socar.CRM.Web.Security;

public sealed class PermissionMiddleware : IMiddleware
{
    private readonly AppDbContext _db;

    public PermissionMiddleware(AppDbContext db)
    {
        _db = db;
    }

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        var endpoint = ctx.GetEndpoint();
        if (endpoint is null)
        {
            await next(ctx);
            return;
        }

        var attr = endpoint.Metadata.GetMetadata<RequirePermissionAttribute>();
        if (attr is null)
        {
            await next(ctx);
            return;
        }

        var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr is null)
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(ctx);
    }
}
