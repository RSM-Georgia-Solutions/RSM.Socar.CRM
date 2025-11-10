using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;

namespace RSM.Socar.CRM.Web.Logging;

public sealed class RequestResponseLoggingMiddleware : IMiddleware
{
    private readonly RequestResponseLoggingOptions _opt;
    private readonly ILogger<RequestResponseLoggingMiddleware> _log;

    public RequestResponseLoggingMiddleware(
        IOptions<RequestResponseLoggingOptions> options,
        ILogger<RequestResponseLoggingMiddleware> log)
    {
        _opt = options.Value;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        if (!_opt.Enabled || ShouldSkip(ctx))
        {
            await next(ctx);
            return;
        }

        var sw = Stopwatch.StartNew();

        // ===== Request =====
        string? reqBody = null;
        try
        {
            if (IsLoggableContentType(ctx.Request.ContentType))
            {
                ctx.Request.EnableBuffering();
                reqBody = await ReadLimitedAsync(ctx.Request.Body, _opt.MaxBodySizeKb);
                ctx.Request.Body.Position = 0;
            }
        }
        catch { /* don't fail the pipeline on logging errors */ }

        // swap response body so we can read it after next()
        var originalBody = ctx.Response.Body;
        await using var buffer = new MemoryStream();
        ctx.Response.Body = buffer;

        Exception? error = null;
        try
        {
            await next(ctx); // execute pipeline
        }
        catch (Exception ex)
        {
            error = ex;
            throw;
        }
        finally
        {
            sw.Stop();

            // ===== Response =====
            string? resBody = null;
            try
            {
                if (IsLoggableContentType(ctx.Response.ContentType))
                {
                    buffer.Position = 0;
                    resBody = await ReadLimitedAsync(buffer, _opt.MaxBodySizeKb);
                }

                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody);
                ctx.Response.Body = originalBody;
            }
            catch { /* swallow logging failures */ }

            // ===== Write structured log =====
            var logLevel = error is null ? LogLevel.Information : LogLevel.Error;

            var props = new Dictionary<string, object?>
            {
                ["Method"] = ctx.Request.Method,
                ["Path"] = ctx.Request.Path.Value,
                ["QueryString"] = ctx.Request.QueryString.Value,
                ["StatusCode"] = ctx.Response.StatusCode,
                ["ElapsedMs"] = sw.Elapsed.TotalMilliseconds,
                ["ClientIP"] = ctx.Connection.RemoteIpAddress?.ToString(),
                ["UserId"] = ctx.User?.FindFirst("sub")?.Value ?? ctx.User?.Identity?.Name,
            };

            if (_opt.IncludeHeaders)
            {
                props["RequestHeaders"] = RedactHeaders(ctx.Request.Headers);
                props["ResponseHeaders"] = ctx.Response.Headers;
            }

            if (!string.IsNullOrWhiteSpace(reqBody)) props["RequestBody"] = reqBody;
            if (!string.IsNullOrWhiteSpace(resBody)) props["ResponseBody"] = resBody;

            if (error is null)
                _log.Log(logLevel, "HTTP {Method} {Path} => {StatusCode} in {ElapsedMs} ms {@Props}",
                    ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, sw.Elapsed.TotalMilliseconds, props);
            else
                _log.Log(logLevel, error, "HTTP {Method} {Path} threw after {ElapsedMs} ms {@Props}",
                    ctx.Request.Method, ctx.Request.Path, sw.Elapsed.TotalMilliseconds, props);
        }
    }

    private static async Task<string> ReadLimitedAsync(Stream s, int maxKb)
    {
        const int KB = 1024;
        var max = maxKb * KB;
        using var ms = new MemoryStream();
        var buffer = new byte[8 * KB];
        int read;
        int total = 0;

        while ((read = await s.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            var toWrite = Math.Min(read, Math.Max(0, max - total));
            if (toWrite > 0)
            {
                ms.Write(buffer, 0, toWrite);
                total += toWrite;
            }
            if (total >= max) break;
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private bool IsLoggableContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return false;
        foreach (var allowed in _opt.IncludeContentTypes)
            if (contentType.StartsWith(allowed, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private bool ShouldSkip(HttpContext ctx)
    {
        var path = ctx.Request.Path.Value ?? "";
        foreach (var p in _opt.ExcludePaths)
            if (path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private static IDictionary<string, string> RedactHeaders(IHeaderDictionary headers)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in headers)
        {
            var key = kv.Key;
            var val = string.Join(",", kv.Value);
            if (key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                val = "***REDACTED***";
            result[key] = val;
        }
        return result;
    }
}
