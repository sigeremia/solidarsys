using SolidarSYS.Application.Relatorios.DTOs;

namespace SolidarSYS.Application.Relatorios.Interfaces;

public interface IRelatorioRepository
{
    Task<int> ContarBeneficiariosAtivosAsync(CancellationToken ct = default);

    /// <summary>Comunidades ativas que possuem ao menos um beneficiário ativo.</summary>
    Task<int> ContarComunidadesAtendidasAsync(CancellationToken ct = default);

    /// <summary>Datas de cadastro dos beneficiários ativos em [desdeUtc, ateUtcExclusivo).</summary>
    Task<IList<DateTime>> ListarDatasCadastroBeneficiariosAsync(DateTime desdeUtc, DateTime ateUtcExclusivo, CancellationToken ct = default);

    /// <summary>Doações com data entre inicio e fim (inclusive).</summary>
    Task<IList<DoacaoPeriodoDto>> ListarDoacoesNoPeriodoAsync(DateOnly inicio, DateOnly fim, CancellationToken ct = default);

    /// <summary>Beneficiários ativos por comunidade ativa, apenas comunidades com ao menos um beneficiário.</summary>
    Task<IList<RelatorioComunidadeDto>> ListarBeneficiariosPorComunidadeAsync(CancellationToken ct = default);
}
