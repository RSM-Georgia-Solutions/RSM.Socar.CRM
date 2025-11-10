using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RSM.Socar.CRM.Domain.Identity;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Infrastructure.Security;

namespace RSM.Socar.CRM.Application.Auth;

public sealed class LoginHandler(
    AppDbContext db,
    IJwtTokenService jwt,
    IOptions<JwtOptions> jwtOpts
) : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        // Uniform error to avoid account enumeration
        if (user is null || user.IsActive == false)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = jwt.CreateAccessToken(user);
        var expires = jwtOpts.Value.AccessTokenMinutes * 60;
        return new LoginResult(token, expires);
    }
}
