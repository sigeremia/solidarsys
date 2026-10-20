namespace SolidarSYS.Application.Beneficiarios.DTOs;

public record BeneficiarioListItemDto(
    int IdBeneficiario,
    string Nome,
    string Cpf,
    DateOnly DataNascimento,
    string Endereco,
    int IdComunidade,
    string Comunidade,
    int TotalMembrosFamilia,
    bool Ativo,
    DateTime DataCriacao);