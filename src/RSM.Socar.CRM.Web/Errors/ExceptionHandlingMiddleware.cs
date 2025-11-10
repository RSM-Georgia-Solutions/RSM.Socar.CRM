using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        // Log: error with enriched properties
        _log.LogError(ex, "Unhandled exception for {Method} {Path} → {Status} (TraceId: {TraceId})",
            ctx.Request.Method, ctx.Request.Path, status, traceId);

        // Prepare ProblemDetails
        var problem = new ProblemDetails
        {
            Title = GetTitleFor(status, ex),
            Status = status,
            Detail = _opt.IncludeExceptionDetails ? ex.Message : null,
            Instance = ctx.Request.Path,
            Type = $"https://httpstatuses.com/{status}"
        };

        // Enrich with trace id (helpful in logs and responses)
        problem.Extensions["traceId"] = traceId;

        // Include error code/info for well-known exceptions (optional)
        if (ex is DbUpdateException dbEx && dbEx.InnerException is not null)
        {
            problem.Extensions["dbError"] = dbEx.InnerException.Message;
        }

        if (_opt.IncludeExceptionDetails && ex.StackTrace is not null)
        {
            problem.Extensions["stackTrace"] = ex.StackTrace;
        }

        // Write JSON ProblemDetails
        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json; charset=utf-8";
        await ctx.Response.WriteAsJsonAsync(problem);
    }

    private int ResolveStatusCode(Exception ex)
    {
        // Exact type match via map
        if (_opt.ExceptionStatusCodeMap.TryGetValue(ex.GetType().FullName!, out var mapped))
            return mapped;

        // Common fallbacks
        return ex switch
        {
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            DbUpdateException => StatusCodes.Status409Conflict,
            KeyNotFoundException => StatusCodes.Status404NotFound,
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
