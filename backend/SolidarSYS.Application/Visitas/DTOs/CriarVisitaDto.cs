namespace SolidarSYS.Application.Visitas.DTOs;

public record CriarVisitaDto(
    int IdBeneficiario,
    DateOnly DataVisita,
    string Responsavel,
    string? Observacoes);
