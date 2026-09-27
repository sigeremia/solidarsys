using SolidarSYS.Application.Setores.DTOs;

namespace SolidarSYS.Application.Setores.Interfaces;

public interface ISetoresService
{
    Task<PagedResultDto<SetoresListItemDto>> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default);
    Task<SetoresListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<SetoresListItemDto> BuscarPorNomeAsync(string nome, CancellationToken ct = default);
    Task<SetoresListItemDto> CriarAsync(CriarSetoresDto dto, CancellationToken ct = default);
    Task<SetoresListItemDto> AtualizarAsync(int id, AtualizarSetoresDto dto, CancellationToken ct = default);
    Task AlternarAtivoAsync(int id, CancellationToken ct = default);
}