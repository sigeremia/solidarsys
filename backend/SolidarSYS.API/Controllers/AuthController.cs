using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SolidarSYS.Application.Auth.DTOs;
using SolidarSYS.Application.Auth.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private const string RefreshTokenCookie = "refreshToken";

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var (response, refreshTokenBruto) = await authService.LoginAsync(request, ct);

        Response.Cookies.Append(RefreshTokenCookie, refreshTokenBruto, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromServices] ITokenService tokenService,
        CancellationToken ct)
    {
        var rawToken = Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(rawToken))
        {
            var hash = tokenService.HashToken(rawToken);
            await authService.LogoutAsync(hash, ct);
        }

        Response.Cookies.Delete(RefreshTokenCookie);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromServices] ITokenService tokenService,
        CancellationToken ct)
    {
        var rawToken = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(rawToken))
            return Unauthorized(new { mensagem = "Refresh token não encontrado." });

        var hash = tokenService.HashToken(rawToken);
        var novoAccessToken = await authService.RefreshAccessTokenAsync(hash, ct);
        return Ok(new { accessToken = novoAccessToken });
    }

    [HttpPost("esqueceu-senha")]
    [EnableRateLimiting("esqueceu-senha")]
    public async Task<IActionResult> EsqueceuSenha([FromBody] EsqueceuSenhaRequestDto request, CancellationToken ct)
    {
        await authService.EsqueceuSenhaAsync(request, ct);
        // Resposta genérica independente do resultado (evita enumeração de usuários)
        return Ok(new { mensagem = "Se o e-mail estiver cadastrado, você receberá as instruções em breve." });
    }

    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequestDto request, CancellationToken ct)
    {
        await authService.RedefinirSenhaAsync(request, ct);
        return Ok(new { mensagem = "Senha redefinida com sucesso. Faça login com sua nova senha." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(idClaim, out var idUsuario))
            return Unauthorized();

        var usuario = await authService.ObterUsuarioAtualAsync(idUsuario, ct);
        return Ok(usuario);
    }
}
