using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Usuarios.DTOs;

public record CriarUsuarioDto(
    string Nome,
    string Email,
    string Senha,
    DateOnly DataNascimento,
    NivelAcesso NivelAcesso,
    int? IdSetor,
    string Cpf);
