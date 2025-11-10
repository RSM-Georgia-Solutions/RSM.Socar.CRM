using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RSM.Socar.CRM.Application.Auth;
using RSM.Socar.CRM.Infrastructure.Extensions;     // <- new
using RSM.Socar.CRM.Infrastructure.Security;
using RSM.Socar.CRM.Web.Endpoints.Auth;
using RSM.Socar.CRM.Web.OData;
using RSM.Socar.CRM.Web.Swagger;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// MediatR (v12+)
builder.Services.AddMediatR(m => m.RegisterServicesFromAssemblyContaining<LoginCommand>());

// Infrastructure (DbContext + JwtOptions + JwtTokenService + PasswordHasher)
// You can pin the migrations assembly to Infrastructure here:
builder.Services.AddInfrastructure(cfg, db =>
    db.UseSqlServer(
        cfg.GetConnectionString("Sql"),
        sql => {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sql.MigrationsAssembly("RSM.Socar.CRM.Infrastructure"); // <- migrations live in Infra
        }));

// JWT Bearer (token validation) — stays in Web because it’s hosting concern
var jwt = cfg.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();


// Controllers + OData
builder.Services
    .AddControllers()
    .AddOData(opt =>
    {
        opt.AddRouteComponents("odata", EdmModelBuilder.GetEdmModel())
           .Select()
           .Filter()
           .OrderBy()
           .Expand()
           .Count()
           .SetMaxTop(100);
    });

builder.Services.AddRouting(); // optional, but good to have with OData


builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c =>
    {
        c.SchemaFilter<DeltaSchemaFilter>();
    });

var app = builder.Build();

// (dev seeding, optional)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RSM.Socar.CRM.Infrastructure.Persistence.AppDbContext>();
    await RSM.Socar.CRM.Infrastructure.Seed.DevUserSeeder.SeedAsync(db);
}

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => "OK");
app.MapAuth();

app.Run();
