namespace SolidarSYS.Application.Beneficiarios.DTOs;

public record CriarBeneficiarioDto(
    string Nome,
    string Cpf,
    DateOnly DataNascimento,
    string Endereco,
    int IdComunidade);