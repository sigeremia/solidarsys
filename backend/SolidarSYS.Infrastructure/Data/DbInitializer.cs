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
        await EnsureBeneficiariosTableExistsAsync(db);
        await EnsureVisitasTableExistsAsync(db);
        await EnsureDoadoresTablesExistAsync(db);

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
    private static async Task EnsureBeneficiariosTableExistsAsync(SolidarSysDbContext db)
    {
    if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
        return;

    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS beneficiarios (
            id_beneficiario INTEGER PRIMARY KEY AUTOINCREMENT,
            nome TEXT NOT NULL,
            cpf TEXT NOT NULL,
            data_nascimento TEXT NOT NULL,
            endereco TEXT NOT NULL,
            id_comunidade INTEGER NOT NULL,
            total_membros_familia INTEGER NOT NULL,
            ativo INTEGER NOT NULL,
            data_criacao TEXT NOT NULL,
            FOREIGN KEY (id_comunidade) REFERENCES comunidades(id_comunidade)
        );
        CREATE UNIQUE INDEX IF NOT EXISTS ix_beneficiarios_cpf ON beneficiarios(cpf);");
}

    private static async Task EnsureVisitasTableExistsAsync(SolidarSysDbContext db)
    {
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
            return;

        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS visitas (
                id_visita INTEGER PRIMARY KEY AUTOINCREMENT,
                id_beneficiario INTEGER NOT NULL,
                data_visita TEXT NOT NULL,
                responsavel TEXT NOT NULL,
                observacoes TEXT,
                data_cadastro TEXT NOT NULL,
                FOREIGN KEY (id_beneficiario) REFERENCES beneficiarios(id_beneficiario)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_visitas_beneficiario_data ON visitas(id_beneficiario, data_visita); ");
    }

    private static async Task EnsureDoadoresTablesExistAsync(SolidarSysDbContext db)
    {
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
            return;

        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS doadores (
                id_doador INTEGER PRIMARY KEY AUTOINCREMENT,
                tipo TEXT NOT NULL,
                nome TEXT NOT NULL,
                nome_fantasia TEXT,
                documento TEXT NOT NULL,
                telefone TEXT,
                email TEXT,
                data_cadastro TEXT NOT NULL,
                observacao TEXT,
                ativo INTEGER NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_doadores_documento ON doadores(documento);
            CREATE TABLE IF NOT EXISTS doacoes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                id_doador INTEGER NOT NULL,
                data TEXT NOT NULL,
                valor NUMERIC NOT NULL,
                descricao TEXT NOT NULL,
                tipo TEXT NOT NULL,
                data_cadastro TEXT NOT NULL,
                FOREIGN KEY (id_doador) REFERENCES doadores(id_doador)
            );
            CREATE INDEX IF NOT EXISTS IX_doacoes_id_doador_data ON doacoes(id_doador, data);");
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

        var setoresDeTeste = new[]
        {
            new Setor
            {
                Nome = "Atendimento Social",
                Descricao = "Acolhimento e acompanhamento de famílias atendidas.",
                Ativo = true,
            },
            new Setor
            {
                Nome = "Saúde Comunitária",
                Descricao = "Ações de prevenção, orientação e apoio em saúde.",
                Ativo = true,
            },
            new Setor
            {
                Nome = "Educação e Capacitação",
                Descricao = "Atividades educativas, oficinas e formação profissional.",
                Ativo = true,
            },
            new Setor
            {
                Nome = "Segurança Alimentar",
                Descricao = "Organização de doações e apoio alimentar às comunidades.",
                Ativo = true,
            },
            new Setor
            {
                Nome = "Comunicação e Mobilização",
                Descricao = "Comunicação institucional e mobilização comunitária.",
                Ativo = true,
            },
            new Setor
            {
                Nome = "Gestão de Voluntariado",
                Descricao = "Integração, orientação e acompanhamento de voluntários.",
                Ativo = true,
            },
            new Setor
            {
                Nome = "Financeiro",
                Descricao = "Controle financeiro, prestação de contas e orçamento.",
                Ativo = true,
            },
        };

        foreach (var setor in setoresDeTeste)
        {
            if (!await db.Setores.AnyAsync(s => s.Nome == setor.Nome))
                db.Setores.Add(setor);
        }

        await db.SaveChangesAsync();

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
            await EnsureBeneficiariosTableExistsAsync(db);
        }

        var comunidadeParaBeneficiarios = await db.Comunidades.FirstOrDefaultAsync(c => c.Ativa);
        if (comunidadeParaBeneficiarios is not null)
        {
            var beneficiariosDemo = new[]
            {
                new Beneficiario
                {
                    Nome = "Maria Silva",
                    Cpf = "12345678909",
                    DataNascimento = new DateOnly(1986, 4, 8),
                    Endereco = "Rua da Esperança, 120",
                    TotalMembrosFamilia = 3,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow,
                },
                new Beneficiario
                {
                    Nome = "João Santos",
                    Cpf = "98765432100",
                    DataNascimento = new DateOnly(1978, 7, 14),
                    Endereco = "Av. Brasil, 44",
                    TotalMembrosFamilia = 2,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow,
                },
                new Beneficiario
                {
                    Nome = "Carlos Souza",
                    Cpf = "45678912355",
                    DataNascimento = new DateOnly(1989, 11, 20),
                    Endereco = "Rua das Flores, 320",
                    TotalMembrosFamilia = 4,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow,
                },
                new Beneficiario
                {
                    Nome = "Ana Oliveira",
                    Cpf = "52998224725",
                    DataNascimento = new DateOnly(1991, 2, 12),
                    Endereco = "Rua das Acácias, 85",
                    TotalMembrosFamilia = 3,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow,
                },
                new Beneficiario
                {
                    Nome = "Pedro Lima",
                    Cpf = "93541134780",
                    DataNascimento = new DateOnly(1983, 8, 21),
                    Endereco = "Avenida Central, 240",
                    TotalMembrosFamilia = 4,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow,
                },
                new Beneficiario
                {
                    Nome = "Luciana Ferreira",
                    Cpf = "11144477735",
                    DataNascimento = new DateOnly(1975, 6, 3),
                    Endereco = "Rua Bela Vista, 17",
                    TotalMembrosFamilia = 2,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow,
                },
                new Beneficiario
                {
                    Nome = "Rafael Costa",
                    Cpf = "39053344705",
                    DataNascimento = new DateOnly(1994, 12, 16),
                    Endereco = "Travessa das Palmeiras, 62",
                    TotalMembrosFamilia = 5,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow,
                },
                new Beneficiario
                {
                    Nome = "Beatriz Almeida",
                    Cpf = "16899535009",
                    DataNascimento = new DateOnly(1988, 3, 29),
                    Endereco = "Rua do Sol, 311",
                    TotalMembrosFamilia = 3,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow,
                },
            };

            foreach (var beneficiario in beneficiariosDemo)
            {
                if (!await db.Beneficiarios.AnyAsync(b => b.Cpf == beneficiario.Cpf))
                {
                    beneficiario.IdComunidade = comunidadeParaBeneficiarios.IdComunidade;
                    db.Beneficiarios.Add(beneficiario);
                }
            }

            await db.SaveChangesAsync();
        }

        var beneficiariosParaVisitas = await db.Beneficiarios.OrderBy(b => b.Nome).ToListAsync();
        if (beneficiariosParaVisitas.Count > 0)
        {
            var visitasDemo = new[]
            {
                (DataVisita: new DateOnly(2024, 9, 30), Responsavel: "Paula Gestora", Observacoes: "Visita de rotina. Família em boas condições."),
                (DataVisita: new DateOnly(2024, 10, 4), Responsavel: "Marcos Operador", Observacoes: "Entrega de cestas básicas."),
                (DataVisita: new DateOnly(2024, 11, 10), Responsavel: "Paula Gestora", Observacoes: "Acompanhamento pós-visita."),
            };

            foreach (var (visitaDemo, indice) in visitasDemo.Select((visita, indice) => (visita, indice)))
            {
                var beneficiario = beneficiariosParaVisitas[indice % beneficiariosParaVisitas.Count];
                var jaExiste = await db.Visitas.AnyAsync(v =>
                    v.IdBeneficiario == beneficiario.IdBeneficiario && v.DataVisita == visitaDemo.DataVisita);

                if (!jaExiste)
                {
                    db.Visitas.Add(new Visita
                    {
                        IdBeneficiario = beneficiario.IdBeneficiario,
                        DataVisita = visitaDemo.DataVisita,
                        Responsavel = visitaDemo.Responsavel,
                        Observacoes = visitaDemo.Observacoes,
                        DataCadastro = DateTime.UtcNow,
                    });
                }
            }

            await db.SaveChangesAsync();
        }

        if (!await db.Doadores.AnyAsync())
        {
            var doadores = new[]
            {
                new Doador
                {
                    Tipo = TipoDoador.PESSOA_FISICA,
                    Nome = "Roberto Almeida (Demo)",
                    Documento = "52998224725",
                    Telefone = "11999991111",
                    Email = "roberto.demo@example.org",
                    DataCadastro = new DateOnly(2026, 1, 10),
                    Ativo = true,
                },
                new Doador
                {
                    Tipo = TipoDoador.PESSOA_JURIDICA,
                    Nome = "Tech Solutions LTDA (Demo)",
                    NomeFantasia = "Tech Solutions",
                    Documento = "11222333000181",
                    Telefone = "1133334444",
                    Email = "contato.demo@example.org",
                    DataCadastro = new DateOnly(2026, 1, 5),
                    Ativo = true,
                },
                new Doador
                {
                    Tipo = TipoDoador.PESSOA_FISICA,
                    Nome = "Fernanda Costa (Demo)",
                    Documento = "11144477735",
                    Telefone = "11988882222",
                    DataCadastro = new DateOnly(2026, 2, 1),
                    Ativo = true,
                },
                new Doador
                {
                    Tipo = TipoDoador.PESSOA_JURIDICA,
                    Nome = "Construtora Horizonte (Demo)",
                    NomeFantasia = "Horizonte",
                    Documento = "EXT-EMPRESA-0004",
                    Telefone = "1122225555",
                    DataCadastro = new DateOnly(2026, 2, 10),
                    Ativo = true,
                },
            };

            db.Doadores.AddRange(doadores);
            await db.SaveChangesAsync();

            db.Doacoes.AddRange(
                new Doacao
                {
                    IdDoador = doadores[0].IdDoador,
                    Data = new DateOnly(2026, 3, 12),
                    Valor = 100,
                    Descricao = "Doação demonstrativa de apoio financeiro",
                    Tipo = TipoDoacao.DINHEIRO,
                    DataCadastro = new DateTimeOffset(2026, 3, 12, 12, 0, 0, TimeSpan.Zero),
                },
                new Doacao
                {
                    IdDoador = doadores[1].IdDoador,
                    Data = new DateOnly(2026, 1, 3),
                    Valor = 250,
                    Descricao = "Doação demonstrativa de alimentos",
                    Tipo = TipoDoacao.ALIMENTOS,
                    DataCadastro = new DateTimeOffset(2026, 1, 3, 12, 0, 0, TimeSpan.Zero),
                },
                new Doacao
                {
                    IdDoador = doadores[2].IdDoador,
                    Data = new DateOnly(2026, 2, 25),
                    Valor = 150,
                    Descricao = "Doação demonstrativa de materiais escolares",
                    Tipo = TipoDoacao.OUTROS,
                    DataCadastro = new DateTimeOffset(2026, 2, 25, 12, 0, 0, TimeSpan.Zero),
                });
            await db.SaveChangesAsync();
        }
    }
}
