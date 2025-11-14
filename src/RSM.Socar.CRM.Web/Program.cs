using Microsoft.EntityFrameworkCore;
using RSM.Socar.CRM.Application.Extensions;
using RSM.Socar.CRM.Infrastructure.Extensions;
using RSM.Socar.CRM.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

var serviceName = "RSM.Socar.CRM.Web";
var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
var envName = builder.Environment.EnvironmentName;

// Layers
builder.AddLoggingLayer();
builder.Services.AddRequestResponseBodyLogging(cfg);   // registers options + IMiddleware
builder.Services.AddExceptionHandlingLayer(cfg);
// register OTel
builder.Services.AddObservabilityLayer(cfg, serviceName, serviceVersion, envName);

builder.Services.AddInfrastructure(cfg, db =>
    db.UseSqlServer(
        cfg.GetConnectionString("Sql"),
        sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsAssembly("RSM.Socar.CRM.Infrastructure");
        }));

builder.Services.AddApplicationLayer();
builder.Services.AddSecurityServices();
builder.Services.AddWebLayer(cfg);

var app = builder.Build();

app.UseRequestLoggingLayer();          // Serilog request summary (optional)
app.UseRequestResponseBodyLogging();   // ADD THIS LINE (must be BEFORE controllers)
app.UseExceptionHandlingLayer();

await app.SeedSecurityDataAsync();
await app.SeedDevDataAsync();

await app.SeedDevDataAsync();
app.UseWebPipeline();                  // this maps controllers/endpoints

app.Run();
