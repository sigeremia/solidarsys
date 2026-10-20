using SolidarSYS.Application.Beneficiarios.DTOs;

namespace SolidarSYS.Application.Beneficiarios.Interfaces;

public interface IBeneficiarioService
{
    Task<PagedResultDto<BeneficiarioListItemDto>> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default);
    Task<BeneficiarioListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<BeneficiarioListItemDto> CriarAsync(CriarBeneficiarioDto dto, CancellationToken ct = default);
    Task<BeneficiarioListItemDto> AtualizarAsync(int id, AtualizarBeneficiarioDto dto, CancellationToken ct = default);
    Task InativarAsync(int id, CancellationToken ct = default);
    Task<IList<ComunidadeOpcaoDto>> ListarComunidadesAsync(CancellationToken ct = default);
}