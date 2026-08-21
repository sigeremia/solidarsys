using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Auth.DTOs;

public record UsuarioInfoDto(
    int Id,
    string Nome,
    string Email,
    NivelAcesso NivelAcesso,
    string? Setor
);
