using SolidarSYS.Application.Relatorios.DTOs;

namespace SolidarSYS.Application.Relatorios.Interfaces;

public interface IRelatorioService
{
    /// <param name="periodo">mensal, trimestral, semestral ou anual. Vazio assume semestral.</param>
    /// <exception cref="ArgumentException">Período não reconhecido.</exception>
    Task<RelatorioDto> ObterAsync(string? periodo, CancellationToken ct = default);
}
