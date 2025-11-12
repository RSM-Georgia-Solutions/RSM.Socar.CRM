using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RSM.Socar.CRM.Application.Abstractions;

namespace RSM.Socar.CRM.Infrastructure.Security;

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private readonly ClaimsPrincipal? _user = accessor.HttpContext?.User;

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;
    public string? UserId => _user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? _user?.FindFirstValue("sub");
    public string? UserName => _user?.Identity?.Name ?? _user?.FindFirstValue("name");
    public string? Email => _user?.FindFirstValue(ClaimTypes.Email) ?? _user?.FindFirstValue("email");
}
