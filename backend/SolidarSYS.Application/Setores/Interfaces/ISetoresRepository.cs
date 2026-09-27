using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Setores.Interfaces;

public interface ISetoresRepository
{
    // ── CRUD comunidades ──────────────────────────────────────────────────────────
    Task<(IList<Setor> Items, int Total)> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativa, CancellationToken ct = default);
    Task<Setor?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Setor> CriarAsync(Setor setor, CancellationToken ct = default);
    Task AtualizarAsync(Setor setor, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);

    // ── Outros métodos ───────────────────────────────────────────────────────────
    Task AlternarAtivoAsync(int id, CancellationToken ct = default);
    Task<Setor?> BuscarPorNomeAsync(string nome, CancellationToken ct = default);
}