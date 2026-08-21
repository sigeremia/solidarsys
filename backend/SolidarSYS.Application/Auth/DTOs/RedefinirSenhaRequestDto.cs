namespace SolidarSYS.Application.Auth.DTOs;

public record RedefinirSenhaRequestDto(string Token, string NovaSenha, string ConfirmarSenha);
