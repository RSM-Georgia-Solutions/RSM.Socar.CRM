using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RSM.Socar.CRM.Application.Abstractions;   // IJwtTokenService (interface living in Application)
using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Auth;


public abstract class LoginCommand
{
    public sealed record Request(string PersonalNo, string Password) : IRequest<Result>;
    public sealed record Result(string AccessToken, int ExpiresInSeconds);


    public sealed class Handler(
        IUserRepository users,
        IPasswordHasher<User> hasher,
        IJwtTokenService jwt,
        IOptions<JwtOptions> jwtOpts
    ) : IRequestHandler<Request, Result>
    {
        public async Task<Result> Handle(Request request, CancellationToken ct)
        {
            // normalize email if you store it normalized; otherwise remove Trim()
            var personalNo = request.PersonalNo?.Trim() ?? string.Empty;

            var user = await users.GetByPersonalNoAsync(personalNo, ct);

            // Uniform error to avoid account enumeration
            if (user is null || user.Status == Domain.Enums.UserStatus.Inactive)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verify == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var token = jwt.CreateAccessToken(user);
            var expiresSeconds = jwtOpts.Value.AccessTokenMinutes * 60;
            return new Result(token, expiresSeconds);
        }
    }
}
