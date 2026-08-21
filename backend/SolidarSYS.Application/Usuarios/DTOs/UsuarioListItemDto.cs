using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Usuarios.DTOs;

public record UsuarioListItemDto(
    int Id,
    string Nome,
    string Email,
    NivelAcesso NivelAcesso,
    string? Setor,
    bool Ativo,
    DateTime DataCriacao);
