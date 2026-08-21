using SolidarSYS.Application.Auth.DTOs;

namespace SolidarSYS.Application.Auth.Interfaces;

public interface IAuthService
{
    // Retorna AccessToken + RefreshTokenBruto (para cookie) + info do usuário
    Task<(LoginResponseDto Response, string RefreshTokenBruto)> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task LogoutAsync(string refreshTokenHash, CancellationToken ct = default);
    Task<string> RefreshAccessTokenAsync(string refreshTokenHash, CancellationToken ct = default);
    Task EsqueceuSenhaAsync(EsqueceuSenhaRequestDto request, CancellationToken ct = default);
    Task RedefinirSenhaAsync(RedefinirSenhaRequestDto request, CancellationToken ct = default);
    Task<UsuarioInfoDto> ObterUsuarioAtualAsync(int idUsuario, CancellationToken ct = default);
}
