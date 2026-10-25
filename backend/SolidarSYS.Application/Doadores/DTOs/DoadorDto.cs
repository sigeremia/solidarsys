using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Doadores.DTOs;

public abstract record DoadorDto(
    TipoDoador Tipo,
    string Nome,
    string Documento,
    string? NomeFantasia,
    string? Telefone,
    string? Email,
    string? Observacao);