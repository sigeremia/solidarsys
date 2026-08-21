using Microsoft.Extensions.Logging;
using SolidarSYS.Application.Auth.DTOs;
using SolidarSYS.Application.Auth.Interfaces;
using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Auth.Services;

public class AuthService(
    IUsuarioRepository usuarioRepo,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<(LoginResponseDto Response, string RefreshTokenBruto)> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Usuário inativo. Contate o administrador.");

        if (usuario.EstaBloqueado())
            throw new UnauthorizedAccessException("Conta bloqueada por excesso de tentativas. Tente novamente em alguns minutos.");

        if (!passwordHasher.Verificar(request.Senha, usuario.Senha))
        {
            usuario.RegistrarTentativaFalha();
            await usuarioRepo.AtualizarAsync(usuario, ct);
            await usuarioRepo.SalvarAlteracoesAsync(ct);
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        usuario.ResetarTentativas();
        usuario.DataUltimoAcesso = DateTime.UtcNow;

        var accessToken = tokenService.GerarAccessToken(usuario);
        var (tokenBruto, tokenHash) = tokenService.GerarRefreshToken();

        var refreshToken = new RefreshToken
        {
            IdUsuario = usuario.IdUsuario,
            Token = tokenHash,
            ExpiraEm = DateTime.UtcNow.AddDays(7)
        };

        await usuarioRepo.AdicionarRefreshTokenAsync(refreshToken, ct);
        await usuarioRepo.AtualizarAsync(usuario, ct);
        await usuarioRepo.SalvarAlteracoesAsync(ct);

        logger.LogInformation("Login bem-sucedido para o usuário {Email}", usuario.Email);

        var response = new LoginResponseDto(accessToken, MapearUsuario(usuario));
        return (response, tokenBruto);
    }

    public async Task LogoutAsync(string refreshTokenHash, CancellationToken ct = default)
    {
        var token = await usuarioRepo.ObterRefreshTokenAsync(refreshTokenHash, ct);
        if (token is null) return;
        token.Revogado = true;
        await usuarioRepo.SalvarAlteracoesAsync(ct);
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshTokenHash, CancellationToken ct = default)
    {
        var token = await usuarioRepo.ObterRefreshTokenAsync(refreshTokenHash, ct)
            ?? throw new UnauthorizedAccessException("Refresh token inválido.");

        if (!token.EstaValido())
            throw new UnauthorizedAccessException("Refresh token expirado ou revogado.");

        var usuario = await usuarioRepo.ObterPorIdAsync(token.IdUsuario, ct)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Usuário inativo.");

        return tokenService.GerarAccessToken(usuario);
    }

    public async Task EsqueceuSenhaAsync(EsqueceuSenhaRequestDto request, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorEmailAsync(request.Email, ct);
        if (usuario is null || !usuario.Ativo)
        {
            logger.LogInformation("Recuperação solicitada para e-mail inválido: {Email}", request.Email);
            return; // Resposta genérica — não revela se o e-mail existe
        }

        var (tokenBruto, tokenHash) = tokenService.GerarTokenRecuperacaoSenha();

        var tokenRecuperacao = new TokenRecuperacaoSenha
        {
            IdUsuario = usuario.IdUsuario,
            Token = tokenHash,
            ExpiraEm = DateTime.UtcNow.AddMinutes(15)
        };

        await usuarioRepo.AdicionarTokenRecuperacaoAsync(tokenRecuperacao, ct);
        await usuarioRepo.SalvarAlteracoesAsync(ct);

        try
        {
            await emailService.EnviarEmailRecuperacaoSenhaAsync(usuario.Email, usuario.Nome, tokenBruto, ct);
            logger.LogInformation("E-mail de recuperação enviado para {Email}", usuario.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar e-mail de recuperação para {Email}. Token salvo no banco.", usuario.Email);
        }
    }

    public async Task RedefinirSenhaAsync(RedefinirSenhaRequestDto request, CancellationToken ct = default)
    {
        var tokenHash = tokenService.HashToken(request.Token);
        var tokenRecuperacao = await usuarioRepo.ObterTokenRecuperacaoAsync(tokenHash, ct)
            ?? throw new InvalidOperationException("Token de recuperação inválido.");

        if (!tokenRecuperacao.EstaValido())
            throw new InvalidOperationException("Token de recuperação expirado ou já utilizado.");

        var usuario = await usuarioRepo.ObterPorIdAsync(tokenRecuperacao.IdUsuario, ct)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        usuario.Senha = passwordHasher.Hash(request.NovaSenha);
        usuario.ResetarTentativas();
        tokenRecuperacao.Usado = true;

        await usuarioRepo.AtualizarAsync(usuario, ct);
        await usuarioRepo.SalvarAlteracoesAsync(ct);

        logger.LogInformation("Senha redefinida com sucesso para o usuário {Id}", usuario.IdUsuario);
    }

    public async Task<UsuarioInfoDto> ObterUsuarioAtualAsync(int idUsuario, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorIdAsync(idUsuario, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");
        return MapearUsuario(usuario);
    }

    private static UsuarioInfoDto MapearUsuario(Usuario u) =>
        new(u.IdUsuario, u.Nome, u.Email, u.NivelAcesso, u.Setor?.Nome);
}
