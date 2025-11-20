using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using RSM.Socar.CRM.Application.Auth;

namespace RSM.Socar.CRM.Web.Endpoints.Auth;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/auth").WithTags("Auth");

        g.MapPost("/login", async Task<Results<Ok<LoginCommand.Result>, UnauthorizedHttpResult>> (
            LoginCommand.Request req, ISender sender, CancellationToken ct) =>
        {
            try
            {
                var res = await sender.Send(req, ct);
                return TypedResults.Ok(res);
            }
            catch (UnauthorizedAccessException)
            {
                return TypedResults.Unauthorized();
            }
        });

        return app;
    }
}

public sealed record LoginRequest(string PersonalNo, string Password);
