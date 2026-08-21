using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SolidarSYS.Application.Auth.Interfaces;
using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Infrastructure.Auth;

public class JwtTokenService(IConfiguration config) : ITokenService
{
    private readonly string _secretKey = config["Jwt:SecretKey"]!;
    private readonly string _issuer = config["Jwt:Issuer"]!;
    private readonly string _audience = config["Jwt:Audience"]!;
    private readonly int _expiracaoMinutos = int.Parse(config["Jwt:AccessTokenExpirationMinutes"]!);

    public string GerarAccessToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.NivelAcesso.ToString()), // usado por [Authorize(Roles=...)]
            new Claim("nivel_acesso", usuario.NivelAcesso.ToString()),  // usado pelo frontend
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiracaoMinutos),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string tokenBruto, string tokenHash) GerarRefreshToken()
    {
        var bruto = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (bruto, HashToken(bruto));
    }

    public (string tokenBruto, string tokenHash) GerarTokenRecuperacaoSenha()
    {
        var bruto = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return (bruto, HashToken(bruto));
    }

    public string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLower();

    public int? ObterIdUsuarioDoToken(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(accessToken)) return null;
        var jwt = handler.ReadJwtToken(accessToken);
        var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }
}
