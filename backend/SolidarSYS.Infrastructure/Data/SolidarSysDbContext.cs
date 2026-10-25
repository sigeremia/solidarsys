using Microsoft.EntityFrameworkCore;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Infrastructure.Data;

public class SolidarSysDbContext(DbContextOptions<SolidarSysDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Setor> Setores => Set<Setor>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<SetorPermissao> SetorPermissoes => Set<SetorPermissao>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TokenRecuperacaoSenha> TokensRecuperacaoSenha => Set<TokenRecuperacaoSenha>();
    public DbSet<Comunidade> Comunidades => Set<Comunidade>();
    public DbSet<Beneficiario> Beneficiarios => Set<Beneficiario>();
    public DbSet<Doador> Doadores => Set<Doador>();
    public DbSet<Doacao> Doacoes => Set<Doacao>();

    private bool IsPostgres => Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enum nativo no PostgreSQL; string no SQLite
        if (IsPostgres)
            modelBuilder.HasPostgresEnum<NivelAcesso>();

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(u => u.IdUsuario);
            e.Property(u => u.IdUsuario).HasColumnName("id_usuario");
            if (IsPostgres) e.Property(u => u.IdUsuario).UseIdentityByDefaultColumn();
            e.Property(u => u.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.Property(u => u.Senha).HasColumnName("senha").HasMaxLength(255).IsRequired();
            e.Property(u => u.DataNascimento).HasColumnName("data_nascimento").IsRequired();
            e.Property(u => u.NivelAcesso).HasColumnName("nivel_acesso").IsRequired();
            if (IsPostgres)
                e.Property(u => u.NivelAcesso).HasColumnType("nivel_acesso");
            else
                e.Property(u => u.NivelAcesso).HasConversion<string>();
            e.Property(u => u.IdSetor).HasColumnName("id_setor");
            e.Property(u => u.Cpf).HasColumnName("cpf").HasMaxLength(11).IsRequired();
            e.Property(u => u.Ativo).HasColumnName("ativo").IsRequired();
            e.Property(u => u.DataCriacao).HasColumnName("data_criacao").IsRequired();
            e.Property(u => u.DataUltimoAcesso).HasColumnName("data_ultimo_acesso");
            e.Property(u => u.TentativasFalhas).HasColumnName("tentativas_falhas").IsRequired();
            e.Property(u => u.BloqueadoAte).HasColumnName("bloqueado_ate");
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Cpf).IsUnique();
            e.HasOne(u => u.Setor).WithMany(s => s.Usuarios).HasForeignKey(u => u.IdSetor);
        });

        modelBuilder.Entity<Setor>(e =>
        {
            e.ToTable("setores");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id");
            if (IsPostgres) e.Property(s => s.Id).UseIdentityByDefaultColumn();
            e.Property(s => s.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
            e.Property(s => s.Descricao).HasColumnName("descricao");
            e.Property(s => s.Ativo).HasColumnName("ativo").IsRequired();
            e.Property(s => s.DataCriacao).HasColumnName("data_criacao").IsRequired();
        });

        modelBuilder.Entity<Permissao>(e =>
        {
            e.ToTable("permissoes");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            if (IsPostgres) e.Property(p => p.Id).UseIdentityByDefaultColumn();
            e.Property(p => p.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
            e.Property(p => p.Descricao).HasColumnName("descricao");
            e.Property(p => p.Recurso).HasColumnName("recurso").HasMaxLength(100).IsRequired();
            e.Property(p => p.Acao).HasColumnName("acao").HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<SetorPermissao>(e =>
        {
            e.ToTable("setor_permissoes");
            e.HasKey(sp => new { sp.IdSetor, sp.IdPermissao });
            e.Property(sp => sp.IdSetor).HasColumnName("id_setor");
            e.Property(sp => sp.IdPermissao).HasColumnName("id_permissao");
            e.HasOne(sp => sp.Setor).WithMany(s => s.SetorPermissoes).HasForeignKey(sp => sp.IdSetor);
            e.HasOne(sp => sp.Permissao).WithMany(p => p.SetorPermissoes).HasForeignKey(sp => sp.IdPermissao);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id");
            if (IsPostgres) e.Property(r => r.Id).UseIdentityByDefaultColumn();
            e.Property(r => r.IdUsuario).HasColumnName("id_usuario").IsRequired();
            e.Property(r => r.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
            e.Property(r => r.ExpiraEm).HasColumnName("expira_em").IsRequired();
            e.Property(r => r.Revogado).HasColumnName("revogado").IsRequired();
            e.Property(r => r.CriadoEm).HasColumnName("criado_em").IsRequired();
            e.HasIndex(r => r.Token).IsUnique();
            e.HasOne(r => r.Usuario).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.IdUsuario);
        });

        modelBuilder.Entity<TokenRecuperacaoSenha>(e =>
        {
            e.ToTable("tokens_recuperacao_senha");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id");
            if (IsPostgres) e.Property(t => t.Id).UseIdentityByDefaultColumn();
            e.Property(t => t.IdUsuario).HasColumnName("id_usuario").IsRequired();
            e.Property(t => t.Token).HasColumnName("token").HasMaxLength(255).IsRequired();
            e.Property(t => t.ExpiraEm).HasColumnName("expira_em").IsRequired();
            e.Property(t => t.Usado).HasColumnName("usado").IsRequired();
            e.Property(t => t.CriadoEm).HasColumnName("criado_em").IsRequired();
            e.HasIndex(t => t.Token).IsUnique();
            e.HasOne(t => t.Usuario).WithMany(u => u.TokensRecuperacao).HasForeignKey(t => t.IdUsuario);
        });

        modelBuilder.Entity<Comunidade>(e =>
        {
            e.ToTable("comunidades");
            e.HasKey(c => c.IdComunidade);
            e.Property(c => c.IdComunidade).HasColumnName("id_comunidade");
            if (IsPostgres) e.Property(c => c.IdComunidade).UseIdentityByDefaultColumn();
            e.Property(c => c.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
            e.Property(c => c.Regiao).HasColumnName("regiao").HasMaxLength(150).IsRequired();
            e.Property(c => c.Descricao).HasColumnName("descricao");
            e.Property(c => c.DataCriacao).HasColumnName("data_criacao").IsRequired();
            e.Property(c => c.Ativa).HasColumnName("ativa").IsRequired();
            e.Property(c => c.TotalBeneficiarios).HasColumnName("total_beneficiarios").IsRequired();
        });
        modelBuilder.Entity<Beneficiario>(e =>
        {
           e.ToTable("beneficiarios");
           e.HasKey(b => b.IdBeneficiario);
           e.Property(b => b.IdBeneficiario).HasColumnName("id_beneficiario");
           if (IsPostgres) e.Property(b => b.IdBeneficiario).UseIdentityByDefaultColumn();
           e.Property(b => b.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
           e.Property(b => b.Cpf).HasColumnName("cpf").HasMaxLength(11).IsRequired();
           e.Property(b => b.DataNascimento).HasColumnName("data_nascimento").IsRequired();
           e.Property(b => b.Endereco).HasColumnName("endereco").HasMaxLength(255).IsRequired();
           e.Property(b => b.IdComunidade).HasColumnName("id_comunidade").IsRequired();
           e.Property(b => b.TotalMembrosFamilia).HasColumnName("total_membros_familia").IsRequired();
           e.Property(b => b.Ativo).HasColumnName("ativo").IsRequired();
           e.Property(b => b.DataCriacao).HasColumnName("data_criacao").IsRequired();
           e.HasIndex(b => b.Cpf).IsUnique();
           e.HasOne(b => b.Comunidade).WithMany().HasForeignKey(b => b.IdComunidade); 
        });

        modelBuilder.Entity<Doador>(e =>
        {
            e.ToTable("doadores");
            e.HasKey(d => d.IdDoador);
            e.Property(d => d.IdDoador).HasColumnName("id_doador");
            if (IsPostgres) e.Property(d => d.IdDoador).UseIdentityByDefaultColumn();
            e.Property(d => d.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(d => d.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            e.Property(d => d.NomeFantasia).HasColumnName("nome_fantasia").HasMaxLength(100);
            e.Property(d => d.Documento).HasColumnName("documento").HasMaxLength(18).IsRequired();
            e.Property(d => d.Telefone).HasColumnName("telefone").HasMaxLength(15);
            e.Property(d => d.Email).HasColumnName("email").HasMaxLength(150);
            e.Property(d => d.DataCadastro).HasColumnName("data_cadastro").IsRequired();
            e.Property(d => d.Observacao).HasColumnName("observacao");
            e.Property(d => d.Ativo).HasColumnName("ativo").IsRequired();
            e.HasIndex(d => d.Documento).IsUnique();
        });

        modelBuilder.Entity<Doacao>(e =>
        {
            e.ToTable("doacoes");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("id");
            if (IsPostgres) e.Property(d => d.Id).UseIdentityByDefaultColumn();
            e.Property(d => d.IdDoador).HasColumnName("id_doador").IsRequired();
            e.Property(d => d.Data).HasColumnName("data").IsRequired();
            e.Property(d => d.Valor).HasColumnName("valor").HasPrecision(12, 2).IsRequired();
            e.Property(d => d.Descricao).HasColumnName("descricao").HasMaxLength(255).IsRequired();
            e.Property(d => d.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(d => d.DataCadastro).HasColumnName("data_cadastro").IsRequired();
            e.HasIndex(d => new { d.IdDoador, d.Data });
            e.HasOne(d => d.Doador).WithMany(doador => doador.Doacoes).HasForeignKey(d => d.IdDoador);
        });
    }
}
