namespace SolidarSYS.Application.Setores.DTOs;

public record SetoresListItemDto(
    int IdSetores,
    bool Ativo,
    string Nome,
    string Descricao,
    DateTime DataCriacao);