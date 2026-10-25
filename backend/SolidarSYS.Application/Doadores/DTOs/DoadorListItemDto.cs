using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Doadores.DTOs;

/// <summary>Representa um doador retornado na listagem.</summary>
public sealed record DoadorListItemDto(
    int IdDoador,
    TipoDoador Tipo,
    string Nome,
    string? NomeFantasia,
    string Documento,
    string? Telefone,
    string? Email,
    DateOnly DataCadastro,
    string? Observacao,
    bool Ativo,
    DateOnly? UltimaDoacao);