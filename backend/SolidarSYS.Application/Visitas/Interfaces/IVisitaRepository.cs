using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Visitas.Interfaces;

public interface IVisitaRepository
{
    Task<(IList<Visita> Items, int Total)> ListarAsync(int pagina, int tamanhoPagina, string? busca, CancellationToken ct = default);
    Task<Visita?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Visita> AdicionarAsync(Visita visita, CancellationToken ct = default);
    Task AtualizarAsync(Visita visita, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
    Task<bool> ExisteVisitaNoDiaAsync(int idBeneficiario, DateOnly dataVisita, int? excluirId = null, CancellationToken ct = default);
    Task<IList<Beneficiario>> ListarBeneficiariosAtivosAsync(CancellationToken ct = default);
    Task<Beneficiario?> ObterBeneficiarioAsync(int idBeneficiario, CancellationToken ct = default);
}
