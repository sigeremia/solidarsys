using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Infrastructure.Data;

public static class DbInitializer
{
    /// <summary>
    /// SQLite (dev): cria schema via EnsureCreated + semeia dados iniciais.
    /// PostgreSQL (prod): aplica migrations pendentes + semeia dados iniciais.
    /// </summary>
    public static async Task InicializarAsync(SolidarSysDbContext db)
    {
        var isPostgres = db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

        if (isPostgres)
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync(); // SQLite: cria tabelas direto do modelo

        await SeedAsync(db);
    }

    private static async Task SeedAsync(SolidarSysDbContext db)
    {
        // Setor administrativo
        if (!await db.Setores.AnyAsync())
        {
            db.Setores.Add(new Setor
            {
                Nome = "Administrativo",
                Descricao = "Setor administrativo principal",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        // Usuário administrador padrão — senha: Admin@123
        if (!await db.Usuarios.AnyAsync(u => u.Email == "admin@solidarsys.org"))
        {
            var setor = await db.Setores.FirstAsync(s => s.Nome == "Administrativo");
            db.Usuarios.Add(new Usuario
            {
                Nome = "Administrador",
                Email = "admin@solidarsys.org",
                Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
                DataNascimento = new DateOnly(1990, 1, 1),
                NivelAcesso = NivelAcesso.ADMINISTRADOR,
                IdSetor = setor.Id,
                Cpf = "00000000000",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                TentativasFalhas = 0,
            });
            await db.SaveChangesAsync();
        }
    }
}
