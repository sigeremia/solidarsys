namespace SolidarSYS.Application.Beneficiarios.DTOs;

public record AtualizarBeneficiarioDto(
    string Nome,
    string Cpf,
    DateOnly DataNascimento,
    string Endereco,
    int IdComunidade);