using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Beneficiarios.Interfaces;

public interface IBeneficiarioRepository
{
    Task<(IList<Beneficiario> Items, int Total)> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default);
    Task<Beneficiario?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteCpfAsync(string cpf, int? excluirId = null, CancellationToken ct = default);
    Task<Beneficiario> AdicionarAsync(Beneficiario beneficiario, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);

    Task<Comunidade?> ObterComunidadeAsync(int idComunidade, CancellationToken ct = default);
    Task<IList<Comunidade>> ListarComunidadesAtivasAsync(CancellationToken ct = default);
    Task AtualizarTotalBeneficiariosAsync(int idComunidade, CancellationToken ct = default);
}