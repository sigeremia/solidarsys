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

        // SQLite pode deixar um arquivo já existente sem todas as tabelas em dev local.stentes.
        await EnsureCommunityTableExistsAsync(db);

        await SeedAsync(db);
    }

    private static async Task EnsureCommunityTableExistsAsync(SolidarSysDbContext db)
    {
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
            return;

        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS comunidades (
                id_comunidade INTEGER PRIMARY KEY AUTOINCREMENT,
                nome TEXT NOT NULL,
                regiao TEXT NOT NULL,
                descricao TEXT,
                data_criacao TEXT NOT NULL,
                ativa INTEGER NOT NULL,
                total_beneficiarios INTEGER NOT NULL
            );");
    }

    private static async Task EnsureSetoresTableExistsAsync(SolidarSysDbContext db) {
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
            return;

        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS setores (
                id_setores INTEGER PRIMARY KEY AUTOINCREMENT,
                nome TEXT NOT NULL,
                descricao TEXT,
                data_criacao TEXT NOT NULL,
                ativo INTEGER NOT NULL
            );");
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

        // Comunidades de exemplo — só criam uma vez, quando a tabela estiver vazia
        if (!await db.Comunidades.AnyAsync())
        {
            db.Comunidades.AddRange(
                new Comunidade
                {
                    Nome = "Comunidade Esperança",
                    Regiao = "São Paulo / SP",
                    Descricao = "Apoio a famílias em vulnerabilidade com cestas básicas e atendimento social.",
                    DataCriacao = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 142,
                },
                new Comunidade
                {
                    Nome = "Casa da Cidadania",
                    Regiao = "Rio de Janeiro / RJ",
                    Descricao = "Projetos de educação, inclusão digital e acolhimento comunitário.",
                    DataCriacao = new DateTime(2023, 11, 8, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 96,
                },
                new Comunidade
                {
                    Nome = "Viva Verde",
                    Regiao = "Belo Horizonte / MG",
                    Descricao = "Horta urbana e reforço de renda para moradores de bairros periféricos.",
                    DataCriacao = new DateTime(2024, 2, 28, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 118,
                },
                new Comunidade
                {
                    Nome = "Mãos Irmãs",
                    Regiao = "Recife / PE",
                    Descricao = "Atendimento psicológico e visitas de apoio a pessoas em situação de risco.",
                    DataCriacao = new DateTime(2023, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = false,
                    TotalBeneficiarios = 67,
                },
                new Comunidade
                {
                    Nome = "Rumo Novo",
                    Regiao = "Porto Alegre / RS",
                    Descricao = "Programa de capacitação profissional e orientação para empregabilidade.",
                    DataCriacao = new DateTime(2024, 4, 3, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 134,
                },
                new Comunidade
                {
                    Nome = "Semear União",
                    Regiao = "Curitiba / PR",
                    Descricao = "Ações de arrecadação, mobilização e desenvolvimento local integrado.",
                    DataCriacao = new DateTime(2023, 12, 19, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 81,
                },
                new Comunidade
                {
                    Nome = "Ponte Solidária",
                    Regiao = "Salvador / BA",
                    Descricao = "Rede de apoio para famílias em vulnerabilidade e eventos comunitários.",
                    DataCriacao = new DateTime(2024, 3, 22, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 153,
                },
                new Comunidade
                {
                    Nome = "Nó na Rede",
                    Regiao = "Fortaleza / CE",
                    Descricao = "Acompanhamento social e encaminhamentos para serviços públicos essenciais.",
                    DataCriacao = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = false,
                    TotalBeneficiarios = 88,
                },
                new Comunidade
                {
                    Nome = "Aliança do Bairro",
                    Regiao = "Goiânia / GO",
                    Descricao = "Ações de convivência, liderança comunitária e apoio emocional.",
                    DataCriacao = new DateTime(2023, 10, 27, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 103,
                },
                new Comunidade
                {
                    Nome = "Conexão Viva",
                    Regiao = "Manaus / AM",
                    Descricao = "Iniciação tecnológica e apoio à educação para jovens da região.",
                    DataCriacao = new DateTime(2024, 5, 17, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 74,
                },
                new Comunidade
                {
                    Nome = "Cuidar e Crescer",
                    Regiao = "Florianópolis / SC",
                    Descricao = "Atendimento a crianças e famílias em contextos de risco social.",
                    DataCriacao = new DateTime(2024, 2, 9, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 129,
                },
                new Comunidade
                {
                    Nome = "Terra de Todos",
                    Regiao = "Natal / RN",
                    Descricao = "Projetos de sustentabilidade, cultura e reforço de vínculos locais.",
                    DataCriacao = new DateTime(2023, 8, 11, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = false,
                    TotalBeneficiarios = 58,
                },
                new Comunidade
                {
                    Nome = "Ações de Luz",
                    Regiao = "Campinas / SP",
                    Descricao = "Apoio a pessoas em situação de rua e distribuição de itens básicos.",
                    DataCriacao = new DateTime(2024, 4, 20, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 162,
                },
                new Comunidade
                {
                    Nome = "Semeando Futuro",
                    Regiao = "Vitória / ES",
                    Descricao = "Oferta de oficinas, mentoria e suporte para microempreendedores locais.",
                    DataCriacao = new DateTime(2023, 11, 23, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 91,
                },
                new Comunidade
                {
                    Nome = "Rede de Apoio",
                    Regiao = "Aracaju / SE",
                    Descricao = "Levantamento de necessidades e ações de acolhimento em comunidade.",
                    DataCriacao = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    Ativa = true,
                    TotalBeneficiarios = 71,
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
