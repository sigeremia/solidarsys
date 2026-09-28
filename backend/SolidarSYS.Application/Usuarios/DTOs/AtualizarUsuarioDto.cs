using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Usuarios.DTOs;

public record AtualizarUsuarioDto(
    string Nome,
    DateOnly DataNascimento,
    NivelAcesso NivelAcesso,
    int? IdSetor);