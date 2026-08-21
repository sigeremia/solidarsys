namespace SolidarSYS.Application.Auth.DTOs;

// Resultado interno do login — inclui o refresh token bruto para o controller setar no cookie.
// Nunca serializar diretamente para o cliente.
internal record LoginInternalResultDto(
    string AccessToken,
    string RefreshTokenBruto,
    UsuarioInfoDto Usuario
);
