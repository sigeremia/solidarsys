using SolidarSYS.Application.Visitas.DTOs;

namespace SolidarSYS.Application.Visitas.Interfaces;

public interface IVisitaService
{
    Task<PagedResultDto<VisitaListItemDto>> ListarAsync(int pagina, int tamanhoPagina, string? busca, CancellationToken ct = default);
    Task<VisitaListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IList<BeneficiarioOpcaoDto>> ListarBeneficiariosAsync(CancellationToken ct = default);
    Task<VisitaListItemDto> CriarAsync(CriarVisitaDto dto, CancellationToken ct = default);
    Task<VisitaListItemDto> AtualizarAsync(int id, AtualizarVisitaDto dto, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
