using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SolidarSYS.API.Middleware;
using SolidarSYS.Application.Auth.Interfaces;
using SolidarSYS.Application.Auth.Services;
using SolidarSYS.Application.Comunidades.Interfaces;
using SolidarSYS.Application.Setores.Interfaces;
using SolidarSYS.Application.Comunidades.Services;
using SolidarSYS.Application.Setores.Services;
using SolidarSYS.Application.Usuarios.Interfaces;
using SolidarSYS.Application.Usuarios.Services;
using SolidarSYS.Domain.Enums;
using SolidarSYS.Infrastructure.Auth;
using SolidarSYS.Infrastructure.Data;
using SolidarSYS.Infrastructure.Email;
using SolidarSYS.Infrastructure.Repositories;
using SolidarSYS.Infrastructure.Setores;
using SolidarSYS.API.Controllers;
using SolidarSYS.Application.Beneficiarios.Interfaces;
using SolidarSYS.Application.Beneficiarios.Services;
using SolidarSYS.Application.Doadores.Interfaces;
using SolidarSYS.Application.Doadores.Services;
using SolidarSYS.Application.Visitas.Interfaces;
using SolidarSYS.Application.Visitas.Services;

var builder = WebApplication.CreateBuilder(args);

// ───── Controllers e validação ─────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<SolidarSYS.Application.Auth.Validators.LoginRequestValidator>();

// ───── Banco de dados — SQLite (dev) ou PostgreSQL (prod) ─────
var dbProvider = builder.Configuration["Database:Provider"] ?? "postgresql";

if (dbProvider == "sqlite")
{
    builder.Services.AddDbContext<SolidarSysDbContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    builder.Services.AddDbContext<SolidarSysDbContext>(opt =>
        opt.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql.MapEnum<NivelAcesso>("nivel_acesso")));
}

// ───── Dependências da aplicação ─────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IComunidadeRepository, ComunidadeRepository>();
builder.Services.AddScoped<ISetoresRepository, SetoresRepository>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IEmailService, GmailSmtpEmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IComunidadeService, ComunidadeService>();
builder.Services.AddScoped<ISetoresService, SetoresService>();
builder.Services.AddScoped<IBeneficiarioRepository, BeneficiarioRepository>();
builder.Services.AddScoped<IBeneficiarioService, BeneficiarioService>();
builder.Services.AddScoped<IVisitaRepository, VisitaRepository>();
builder.Services.AddScoped<IVisitaService, VisitaService>();
builder.Services.AddScoped<IDoadorRepository, DoadorRepository>();
builder.Services.AddScoped<IDoadorService, DoadorService>();


// ───── JWT Authentication ─────
var jwtKey = builder.Configuration["Jwt:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ───── Rate Limiting (nativo ASP.NET Core) ─────
builder.Services.AddRateLimiter(options =>
{
    // Login: 10 req/min por IP
    options.AddSlidingWindowLimiter("login", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });

    // Esqueceu senha: 3 req/hora por IP
    options.AddSlidingWindowLimiter("esqueceu-senha", opt =>
    {
        opt.Window = TimeSpan.FromHours(1);
        opt.SegmentsPerWindow = 6;
        opt.PermitLimit = 3;
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
});

// ───── CORS ─────
var frontendOrigins = new[]
{
    builder.Configuration["App:FrontendUrl"] ?? "http://localhost:4200",
    "http://localhost:4200",
    "http://127.0.0.1:4200",
    "https://solidarsys-1.onrender.com",
};

builder.Services.AddCors(options =>
    options.AddPolicy("SolidarSYS", policy =>
        policy.WithOrigins(frontendOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ───── OpenAPI (Scalar) ─────
builder.Services.AddOpenApi();

var app = builder.Build();

// ───── Inicialização do banco ─────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SolidarSysDbContext>();
    await DbInitializer.InicializarAsync(db);
}

// ───── Pipeline HTTP ─────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "SolidarSYS API";
        opt.Theme = ScalarTheme.BluePlanet;
        opt.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);
    });
}
app.UseCors("SolidarSYS");
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
