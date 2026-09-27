using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Comunidades.Interfaces;

public interface IComunidadeRepository
{
    // ── CRUD comunidades ──────────────────────────────────────────────────────────
    Task<(IList<Comunidade> Items, int Total)> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativa, CancellationToken ct = default);
    Task<Comunidade?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Comunidade> CriarAsync(Comunidade comunidade, CancellationToken ct = default);
    Task AtualizarAsync(Comunidade comunidade, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);

    // ── Outros métodos ───────────────────────────────────────────────────────────
    Task AlternarAtivoAsync(int id, CancellationToken ct = default);
    Task<Comunidade?> BuscarPorNomeAsync(string nome, CancellationToken ct = default);
}