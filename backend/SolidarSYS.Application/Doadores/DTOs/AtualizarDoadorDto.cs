using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Doadores.DTOs;

/// <summary>Dados para atualizar um doador existente.</summary>
public sealed record AtualizarDoadorDto(
    TipoDoador Tipo,
    string Nome,
    string Documento,
    string? NomeFantasia,
    string? Telefone,
    string? Email,
    string? Observacao)
    : DoadorDto(Tipo, Nome, Documento, NomeFantasia, Telefone, Email, Observacao);