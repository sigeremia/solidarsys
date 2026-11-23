using SolidarSYS.Application.Relatorios.DTOs;
using SolidarSYS.Application.Relatorios.Interfaces;

namespace SolidarSYS.Application.Relatorios.Services;

public class RelatorioService(IRelatorioRepository repo) : IRelatorioService
{
    private const string PeriodoPadrao = "semestral";

    // Quantidade de meses (incluindo o mês atual) de cada período.
    private static readonly Dictionary<string, int> MesesPorPeriodo = new()
    {
        ["mensal"] = 1,
        ["trimestral"] = 3,
        ["semestral"] = 6,
        ["anual"] = 12,
    };

    public async Task<RelatorioDto> ObterAsync(string? periodo, CancellationToken ct = default)
    {
        var chave = string.IsNullOrWhiteSpace(periodo) ? PeriodoPadrao : periodo.Trim().ToLowerInvariant();
        if (!MesesPorPeriodo.TryGetValue(chave, out var meses))
            throw new ArgumentException("Período inválido. Use mensal, trimestral, semestral ou anual.");

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioMes = new DateOnly(hoje.Year, hoje.Month, 1);
        var porDia = meses == 1;
        var inicio = inicioMes.AddMonths(-(meses - 1));
        var fim = hoje;

        var inicioUtc = new DateTime(inicio.Year, inicio.Month, inicio.Day, 0, 0, 0, DateTimeKind.Utc);
        var fimUtcExclusivo = new DateTime(fim.Year, fim.Month, fim.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);

        // DbContext não é thread-safe: as consultas rodam em sequência.
        var totalBeneficiarios = await repo.ContarBeneficiariosAtivosAsync(ct);
        var comunidadesAtendidas = await repo.ContarComunidadesAtendidasAsync(ct);
        var comunidades = await repo.ListarBeneficiariosPorComunidadeAsync(ct);
        var cadastros = (await repo.ListarDatasCadastroBeneficiariosAsync(inicioUtc, fimUtcExclusivo, ct))
            .Select(d => DateOnly.FromDateTime(d))
            .ToList();
        var doacoes = await repo.ListarDoacoesNoPeriodoAsync(inicio, fim, ct);

        var serie = new List<RelatorioPontoDto>();
        if (porDia)
        {
            for (var dia = inicio; dia <= fim; dia = dia.AddDays(1))
            {
                serie.Add(new RelatorioPontoDto(
                    dia,
                    cadastros.Count(d => d == dia),
                    doacoes.Where(d => d.Data == dia).Sum(d => d.Valor)));
            }
        }
        else
        {
            for (var i = 0; i < meses; i++)
            {
                var mes = inicio.AddMonths(i);
                serie.Add(new RelatorioPontoDto(
                    mes,
                    cadastros.Count(d => d.Year == mes.Year && d.Month == mes.Month),
                    doacoes.Where(d => d.Data.Year == mes.Year && d.Data.Month == mes.Month).Sum(d => d.Valor)));
            }
        }

        return new RelatorioDto(
            chave,
            porDia ? "DIA" : "MES",
            inicio,
            fim,
            totalBeneficiarios,
            doacoes.Sum(d => d.Valor),
            comunidadesAtendidas,
            cadastros.Count == 0 && doacoes.Count == 0,
            serie,
            comunidades);
    }
}
