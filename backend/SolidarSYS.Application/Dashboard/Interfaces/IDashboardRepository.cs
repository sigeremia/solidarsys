using SolidarSYS.Application.Dashboard.DTOs;

namespace SolidarSYS.Application.Dashboard.Interfaces;

public interface IDashboardRepository
{
    Task<int> ContarBeneficiariosAtivosAsync(CancellationToken ct = default);
    Task<int> ContarDoadoresAtivosAsync(CancellationToken ct = default);
    Task<int> ContarComunidadesAtivasAsync(CancellationToken ct = default);
    Task<int> ContarVisitasNoPeriodoAsync(DateOnly inicio, DateOnly fim, CancellationToken ct = default);
    Task<IList<DateTime>> ListarDatasCadastroBeneficiariosAsync(DateTime desdeUtc, CancellationToken ct = default);
    Task<IList<DateOnly>> ListarDatasCadastroDoadoresAsync(DateOnly desde, CancellationToken ct = default);
    Task<IList<ComunidadeResumoDto>> ListarBeneficiariosPorComunidadeAsync(CancellationToken ct = default);
}
