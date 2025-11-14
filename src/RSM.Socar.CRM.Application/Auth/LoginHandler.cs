using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RSM.Socar.CRM.Application.Abstractions;   // IJwtTokenService (interface living in Application)
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Auth;

public sealed class LoginHandler(
    IUserRepository users,
    IPasswordHasher<User> hasher,
    IJwtTokenService jwt,
    IOptions<JwtOptions> jwtOpts
) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        // normalize email if you store it normalized; otherwise remove Trim()
        var email = request.Email?.Trim() ?? string.Empty;

        var user = await users.GetByEmailAsync(email, ct);

        // Uniform error to avoid account enumeration
        if (user is null || user.IsActive == false)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = jwt.CreateAccessToken(user);
        var expiresSeconds = jwtOpts.Value.AccessTokenMinutes * 60;
        return new LoginResult(token, expiresSeconds);
    }
}
