namespace SolidarSYS.Application.Auth.Interfaces;

public interface IEmailService
{
    Task EnviarEmailRecuperacaoSenhaAsync(string destinatario, string nomeUsuario, string linkRecuperacao, CancellationToken ct = default);
}
