namespace SolidarSYS.Application.Relatorios.DTOs;

/// <summary>Ponto da série temporal (um dia ou um mês, conforme a granularidade).</summary>
public record RelatorioPontoDto(DateOnly Data, int Beneficiarios, decimal Doacoes);

public record RelatorioComunidadeDto(string Nome, int TotalBeneficiarios);

/// <summary>Doação reduzida ao necessário para consolidar o relatório.</summary>
public record DoacaoPeriodoDto(DateOnly Data, decimal Valor);

/// <param name="Periodo">mensal | trimestral | semestral | anual</param>
/// <param name="Granularidade">DIA (período mensal) ou MES (demais períodos).</param>
/// <param name="SemDados">true quando não há cadastros nem doações no período selecionado.</param>
public record RelatorioDto(
    string Periodo,
    string Granularidade,
    DateOnly Inicio,
    DateOnly Fim,
    int TotalBeneficiarios,
    decimal TotalDoacoes,
    int ComunidadesAtendidas,
    bool SemDados,
    IList<RelatorioPontoDto> Serie,
    IList<RelatorioComunidadeDto> Comunidades);
