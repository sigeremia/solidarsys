using SolidarSYS.Application.Dashboard.DTOs;
using SolidarSYS.Application.Dashboard.Interfaces;

namespace SolidarSYS.Application.Dashboard.Services;

public class DashboardService(IDashboardRepository repo) : IDashboardService
{
    private const int MesesEvolucao = 6;

    public async Task<DashboardDto> ObterAsync(bool incluirDoadores, CancellationToken ct = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioMes = new DateOnly(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);
        var inicioJanela = inicioMes.AddMonths(-(MesesEvolucao - 1));
        var inicioJanelaUtc = new DateTime(inicioJanela.Year, inicioJanela.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // DbContext não é thread-safe: as consultas rodam em sequência.
        var totalBeneficiarios = await repo.ContarBeneficiariosAtivosAsync(ct);
        var totalComunidades = await repo.ContarComunidadesAtivasAsync(ct);
        var visitasMes = await repo.ContarVisitasNoPeriodoAsync(inicioMes, fimMes, ct);
        var datasBeneficiarios = await repo.ListarDatasCadastroBeneficiariosAsync(inicioJanelaUtc, ct);
        var comunidades = await repo.ListarBeneficiariosPorComunidadeAsync(ct);

        int? totalDoadores = null;
        IList<DateOnly> datasDoadores = new List<DateOnly>();
        if (incluirDoadores)
        {
            totalDoadores = await repo.ContarDoadoresAtivosAsync(ct);
            datasDoadores = await repo.ListarDatasCadastroDoadoresAsync(inicioJanela, ct);
        }

        var evolucao = new List<EvolucaoMensalDto>();
        for (var i = 0; i < MesesEvolucao; i++)
        {
            var mes = inicioJanela.AddMonths(i);
            var beneficiarios = datasBeneficiarios.Count(d => d.Year == mes.Year && d.Month == mes.Month);
            int? doadores = incluirDoadores
                ? datasDoadores.Count(d => d.Year == mes.Year && d.Month == mes.Month)
                : null;
            evolucao.Add(new EvolucaoMensalDto(mes.Year, mes.Month, beneficiarios, doadores));
        }

        return new DashboardDto(
            totalBeneficiarios,
            totalDoadores,
            visitasMes,
            totalComunidades,
            evolucao,
            comunidades);
    }
}
