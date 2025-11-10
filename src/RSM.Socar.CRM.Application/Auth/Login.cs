using MediatR;

namespace RSM.Socar.CRM.Application.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(string AccessToken, int ExpiresInSeconds);
