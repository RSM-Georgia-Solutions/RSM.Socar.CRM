namespace RSM.Socar.CRM.Web.Logging;

public sealed class RequestResponseLoggingOptions
{
    public bool Enabled { get; set; } = true;

    // Only log these content types (default: JSON & OData JSON)
    public string[] IncludeContentTypes { get; set; } =
    [
        "application/json",
        "application/*+json",
        "application/prs.odatatestxx-odata",
        "application/odata",
        "application/odata+json"
    ];

    // Skip these paths (swagger, health, etc.)
    public string[] ExcludePaths { get; set; } =
    [
        "/health",
        "/swagger",
        "/swagger/index.html",
        "/swagger/v1/swagger.json"
    ];

    public int MaxBodySizeKb { get; set; } = 256; // truncate beyond this
    public bool IncludeHeaders { get; set; } = false; // toggle if you want headers too
}
