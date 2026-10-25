using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Doadores.Interfaces;

public interface IDoadorRepository
{
    Task<(IList<Doador> Items, int Total)> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default);
    Task<Doador?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken ct = default);
    Task AdicionarAsync(Doador doador, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}