using Microsoft.EntityFrameworkCore;
using SolidarSYS.Application.Auth.Interfaces;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Infrastructure.Data;

namespace SolidarSYS.Infrastructure.Repositories;

public class UsuarioRepository(SolidarSysDbContext db) : IUsuarioRepository
{
    // ── Auth ───────────────────────────────────────────────────────────────────

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct) =>
        db.Usuarios.Include(u => u.Setor).FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct) =>
        db.Usuarios.Include(u => u.Setor).FirstOrDefaultAsync(u => u.IdUsuario == id, ct);

    public Task AtualizarAsync(Usuario usuario, CancellationToken ct)
    {
        db.Usuarios.Update(usuario);
        return Task.CompletedTask;
    }

    public Task<RefreshToken?> ObterRefreshTokenAsync(string tokenHash, CancellationToken ct) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenHash, ct);

    public async Task AdicionarRefreshTokenAsync(RefreshToken token, CancellationToken ct) =>
        await db.RefreshTokens.AddAsync(token, ct);

    public Task<TokenRecuperacaoSenha?> ObterTokenRecuperacaoAsync(string tokenHash, CancellationToken ct) =>
        db.TokensRecuperacaoSenha.FirstOrDefaultAsync(t => t.Token == tokenHash, ct);

    public async Task AdicionarTokenRecuperacaoAsync(TokenRecuperacaoSenha token, CancellationToken ct) =>
        await db.TokensRecuperacaoSenha.AddAsync(token, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    // ── CRUD usuários ──────────────────────────────────────────────────────────

    public async Task<(IList<Usuario> Items, int Total)> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct)
    {
        var query = db.Usuarios.Include(u => u.Setor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.ToLower();
            query = query.Where(u => u.Nome.ToLower().Contains(termo) || u.Email.ToLower().Contains(termo));
        }

        if (ativo.HasValue)
            query = query.Where(u => u.Ativo == ativo.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<bool> ExisteEmailAsync(string email, int? excluirId, CancellationToken ct) =>
        db.Usuarios.AnyAsync(u => u.Email == email.ToLower() && u.IdUsuario != excluirId, ct);

    public Task<bool> ExisteCpfAsync(string cpf, int? excluirId, CancellationToken ct) =>
        db.Usuarios.AnyAsync(u => u.Cpf == cpf && u.IdUsuario != excluirId, ct);

    public async Task<Usuario> AdicionarAsync(Usuario usuario, CancellationToken ct)
    {
        await db.Usuarios.AddAsync(usuario, ct);
        return usuario;
    }
}
