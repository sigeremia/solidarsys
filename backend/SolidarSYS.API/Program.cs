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
using SolidarSYS.Application.Comunidades.Services;
using SolidarSYS.Application.Usuarios.Interfaces;
using SolidarSYS.Application.Usuarios.Services;
using SolidarSYS.Infrastructure.Auth;
using SolidarSYS.Infrastructure.Data;
using SolidarSYS.Infrastructure.Email;
using SolidarSYS.Infrastructure.Repositories;

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
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// ───── Dependências da aplicação ─────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IComunidadeRepository, ComunidadeRepository>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IEmailService, GmailSmtpEmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IComunidadeService, ComunidadeService>();

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
builder.Services.AddCors(opt =>
    opt.AddPolicy("SolidarSYS", policy =>
        policy.WithOrigins(builder.Configuration["App:FrontendUrl"] ?? "http://localhost:4200")
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

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors("SolidarSYS");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
