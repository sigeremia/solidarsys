using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Auth.Interfaces;

public interface ITokenService
{
    string GerarAccessToken(Usuario usuario);
    (string tokenBruto, string tokenHash) GerarRefreshToken();
    (string tokenBruto, string tokenHash) GerarTokenRecuperacaoSenha();
    string HashToken(string token);
    int? ObterIdUsuarioDoToken(string accessToken);
}
