using SolidarSYS.Application.Beneficiarios.DTOs;
using SolidarSYS.Application.Doadores.DTOs;

namespace SolidarSYS.Application.Doadores.Interfaces;

public interface IDoadorService
{
    Task<PagedResultDto<DoadorListItemDto>> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default);
    Task<DoadorListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<DoadorListItemDto> CriarAsync(CriarDoadorDto dto, CancellationToken ct = default);
    Task<DoadorListItemDto> AtualizarAsync(int id, AtualizarDoadorDto dto, CancellationToken ct = default);
    Task InativarAsync(int id, CancellationToken ct = default);
}