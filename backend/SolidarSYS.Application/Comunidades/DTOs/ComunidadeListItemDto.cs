namespace SolidarSYS.Application.Comunidades.DTOs;

public record ComunidadeListItemDto(
    int IdComunidade,
    bool Ativa,
    string Nome,
    string Regiao,
    string Descricao,
    DateTime DataCriacao,
    int TotalBeneficiarios);