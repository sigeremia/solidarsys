using SolidarSYS.Application.Usuarios.DTOs;

namespace SolidarSYS.Application.Usuarios.Interfaces;

public interface IUsuarioService
{
    Task<PagedResultDto<UsuarioListItemDto>> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default);
    Task<UsuarioListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<UsuarioListItemDto> CriarAsync(CriarUsuarioDto dto, CancellationToken ct = default);
    Task<UsuarioListItemDto> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct = default);
    Task AlternarAtivoAsync(int id, CancellationToken ct = default);
}