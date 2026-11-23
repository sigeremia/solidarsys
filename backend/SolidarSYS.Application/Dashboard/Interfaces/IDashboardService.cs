using SolidarSYS.Application.Dashboard.DTOs;

namespace SolidarSYS.Application.Dashboard.Interfaces;

public interface IDashboardService
{
    /// <param name="incluirDoadores">false para perfis sem acesso a doadores (OPERADOR).</param>
    Task<DashboardDto> ObterAsync(bool incluirDoadores, CancellationToken ct = default);
}
