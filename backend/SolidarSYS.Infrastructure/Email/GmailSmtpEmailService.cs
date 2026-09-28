using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SolidarSYS.Application.Auth.Interfaces;

namespace SolidarSYS.Infrastructure.Email;

public class GmailSmtpEmailService(IConfiguration config, ILogger<GmailSmtpEmailService> logger) : IEmailService
{
    private readonly string _host = config["Email:Host"]!;
    private readonly int _port = int.Parse(config["Email:Port"]!);
    private readonly string _username = config["Email:Username"]!;
    private readonly string _password = config["Email:Password"]!;
    private readonly string _nomeRemetente = config["Email:NomeRemetente"]!;
    private readonly string _urlFrontend = config["Email:UrlFrontend"]!;

    public async Task EnviarEmailRecuperacaoSenhaAsync(
        string destinatario,
        string nomeUsuario,
        string tokenBruto,
        CancellationToken ct = default)
    {
        var link = $"{_urlFrontend.TrimEnd('/')}/redefinir-senha?token={Uri.EscapeDataString(tokenBruto)}";

        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(_nomeRemetente, _username));
        mensagem.To.Add(MailboxAddress.Parse(destinatario));
        mensagem.Subject = "SolidarSYS — Recuperação de Senha";
        var corpo = new BodyBuilder
        {
            HtmlBody = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
                  <h2 style="color:#1a56db">SolidarSYS</h2>
                  <p>Olá, <strong>{nomeUsuario}</strong>!</p>
                  <p>Recebemos uma solicitação para redefinir sua senha.</p>
                  <p>Clique no botão abaixo para criar uma nova senha. Este link é válido por <strong>15 minutos</strong>.</p>
                  <a href="{link}"
                     style="display:inline-block;padding:12px 24px;background:#1a56db;color:#fff;text-decoration:none;border-radius:6px;font-weight:bold">
                    Redefinir senha
                  </a>
                  <p style="margin-top:24px;color:#6b7280;font-size:14px">
                    Se você não solicitou a recuperação de senha, ignore este e-mail.<br>
                    O link expirará automaticamente.
                  </p>
                  <hr style="border:none;border-top:1px solid #e5e7eb">
                  <p style="color:#9ca3af;font-size:12px">SolidarSYS — Sistema de Gestão para ONGs</p>
                </div>
                """
        };
        Console.WriteLine($"EMAIL: {_username}");
        Console.WriteLine($"PASSWORD LENGTH: {_password.Length}");
        mensagem.Body = corpo.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls, ct);
        Console.WriteLine($"EMAIL: {_username}");
        Console.WriteLine($"PASSWORD LENGTH: {_password.Length}");
        await smtp.AuthenticateAsync(_username, _password, ct);
        await smtp.SendAsync(mensagem, ct);
        await smtp.DisconnectAsync(quit: true, ct);

        logger.LogInformation("E-mail de recuperação enviado para {Email}", destinatario);
    }
}
