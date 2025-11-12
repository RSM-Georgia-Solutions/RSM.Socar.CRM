using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace RSM.Socar.CRM.Web.Errors;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _log;
    private readonly ExceptionHandlingOptions _opt;

    public ExceptionHandlingMiddleware(
        ILogger<ExceptionHandlingMiddleware> log,
        IOptions<ExceptionHandlingOptions> options)
    {
        _log = log;
        _opt = options.Value;
    }

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            await HandleAsync(ctx, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var status = ResolveStatusCode(ex);
        var traceId = ctx.TraceIdentifier;

        var isClientError = status is >= 400 and < 500;
        var showDetails = _opt.IncludeExceptionDetails || isClientError;

        var problem = new ProblemDetails
        {
            Title = GetTitleFor(status, ex),
            Status = status,
            Detail = showDetails ? ex.Message : null,   // ← only show for 4xx or when enabled
            Instance = ctx.Request.Path,
            Type = $"https://httpstatuses.com/{status}"
        };
        problem.Extensions["traceId"] = traceId;

        if (ex is FluentValidation.ValidationException vex)
        {
            problem.Extensions["errors"] = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json; charset=utf-8";
        await ctx.Response.WriteAsJsonAsync(problem);
    }


    private int ResolveStatusCode(Exception ex)
    {
        if (_opt.ExceptionStatusCodeMap.TryGetValue(ex.GetType().FullName!, out var mapped))
            return mapped;

        return ex switch
        {
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            InvalidOperationException => StatusCodes.Status409Conflict, // e.g., duplicates
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }


    private static string GetTitleFor(int status, Exception ex) => status switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "An unexpected error occurred"
    };
}
