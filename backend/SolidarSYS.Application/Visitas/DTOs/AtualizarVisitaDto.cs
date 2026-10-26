namespace SolidarSYS.Application.Visitas.DTOs;

public record AtualizarVisitaDto(
    int IdBeneficiario,
    DateOnly DataVisita,
    string Responsavel,
    string? Observacoes);
