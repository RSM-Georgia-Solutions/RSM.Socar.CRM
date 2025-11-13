using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public sealed class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string correlationId = Guid.NewGuid().ToString("N");
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception. Path: {Path}, CorrelationId: {CorrelationId}",
                context.Request.Path,
                correlationId);

            await WriteProblemDetails(context, ex, correlationId);
        }
    }

    private static async Task WriteProblemDetails(
    HttpContext context,
    Exception ex,
    string correlationId)
    {
        var problem = ex switch
        {
            UnauthorizedAccessException => new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "Access denied.",
                Type = "https://httpstatuses.com/401"
            },

            KeyNotFoundException => new ProblemDetails
            {
                Title = "Not Found",
                Status = 404,
                Detail = ex.Message,
                Type = "https://httpstatuses.com/404"
            },

            DbUpdateConcurrencyException => new ProblemDetails
            {
                Title = "Concurrency Conflict",
                Status = 409,
                Detail = "The resource was modified by another request.",
                Type = "https://yourdomain.com/errors/concurrency"
            },

            DbUpdateException dbex when IsUniqueConstraintViolation(dbex) => new ProblemDetails
            {
                Title = "Duplicate Value",
                Status = 409,
                Detail = ExtractConstraintMessage(dbex),
                Type = "https://yourdomain.com/errors/duplicate"
            },


            FluentValidation.ValidationException v =>
                new ValidationProblemDetails(ToModelState(v))
                {
                    Status = 400,
                    Title = "Validation Failed",
                    Type = "https://yourdomain.com/errors/validation"
                },

            _ => new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = 500,
                Detail = ex.Message,
                Type = "https://httpstatuses.com/500"
            }
        };

        problem.Extensions["correlationId"] = correlationId;

        context.Response.StatusCode = problem.Status ?? 500;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(json);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is not SqlException sqlEx)
            return false;

        return sqlEx.Number is 2601 or 2627;
    }

    private static string ExtractConstraintMessage(DbUpdateException ex)
    {
        // You can customize mapping to friendly field names
        if (ex.InnerException is SqlException sqlEx)
        {
            if (sqlEx.Number is 2601 or 2627)
                return "A record with the same unique value already exists.";
        }

        return "Database update failed.";
    }


    private static ModelStateDictionary ToModelState(ValidationException exception)
    {
        var modelState = new ModelStateDictionary();

        foreach (var error in exception.Errors)
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);

        return modelState;
    }
}
