using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RSM.Socar.CRM.Application.Auth;
using RSM.Socar.CRM.Infrastructure.Persistence;
using RSM.Socar.CRM.Infrastructure.Security;
using RSM.Socar.CRM.Web.Endpoints.Auth;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// EF Core
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(cfg.GetConnectionString("Sql"))
     .EnableRetryOnFailure());

// MediatR
builder.Services.AddMediatR(typeof(LoginCommand).Assembly);

// JWT options + auth
builder.Services.Configure<JwtOptions>(cfg.GetSection(JwtOptions.SectionName));
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

// JWT creator
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

var app = builder.Build();


// in Program.cs, after building app:
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await RSM.Socar.CRM.Infrastructure.Seed.DevUserSeeder.SeedAsync(db);
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => "OK");

// Map endpoints
app.MapAuth();

app.Run();
