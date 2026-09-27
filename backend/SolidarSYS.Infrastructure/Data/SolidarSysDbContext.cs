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
    }
}
