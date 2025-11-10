namespace RSM.Socar.CRM.Web.Errors;

public sealed class ExceptionHandlingOptions
{
    /// Show exception details & stack traces in responses (typically true only in Development).
    public bool IncludeExceptionDetails { get; set; } = false;

    /// Map of exception type (FullName) → HTTP status code.
    public Dictionary<string, int> ExceptionStatusCodeMap { get; set; } = new()
    {
        { typeof(UnauthorizedAccessException).FullName!, 401 },
        { "RSM.Socar.CRM.Domain.Common.ForbiddenException", 403 },   // sample custom types
        { "RSM.Socar.CRM.Domain.Common.NotFoundException", 404 },
        { typeof(ArgumentException).FullName!,             400 },
        { typeof(ArgumentNullException).FullName!,         400 },
        { typeof(InvalidOperationException).FullName!,     409 },
    };
}
