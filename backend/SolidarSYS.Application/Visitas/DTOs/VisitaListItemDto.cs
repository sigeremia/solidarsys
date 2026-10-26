namespace SolidarSYS.Application.Visitas.DTOs;

public record VisitaListItemDto(
    int IdVisita,
    int IdBeneficiario,
    string Beneficiario,
    DateOnly DataVisita,
    string Responsavel,
    string? Observacoes,
    DateTime DataCadastro);
