using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Usuarios.DTOs;

public record UsuarioListItemDto(
    int Id,
    string Nome,
    string Email,
    DateOnly DataNascimento,
    NivelAcesso NivelAcesso,
    string? Setor,
    bool Ativo,
    DateTime DataCriacao);