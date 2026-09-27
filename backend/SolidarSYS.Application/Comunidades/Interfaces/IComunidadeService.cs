using SolidarSYS.Application.Comunidades.DTOs;

namespace SolidarSYS.Application.Comunidades.Interfaces;

public interface IComunidadeService
{
    Task<PagedResultDto<ComunidadeListItemDto>> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default);
    Task<ComunidadeListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<ComunidadeListItemDto> BuscarPorNomeAsync(string nome, CancellationToken ct = default);
    Task<ComunidadeListItemDto> CriarAsync(CriarComunidadeDto dto, CancellationToken ct = default);
    Task<ComunidadeListItemDto> AtualizarAsync(int id, AtualizarComunidadeDto dto, CancellationToken ct = default);
    Task AlternarAtivoAsync(int id, CancellationToken ct = default);
}