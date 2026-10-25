using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Doadores.DTOs;

/// <summary>Dados necessários para cadastrar um doador.</summary>
public sealed record CriarDoadorDto(
    TipoDoador Tipo,
    string Nome,
    string Documento,
    string? NomeFantasia,
    string? Telefone,
    string? Email,
    string? Observacao)
    : DoadorDto(Tipo, Nome, Documento, NomeFantasia, Telefone, Email, Observacao);