using Microsoft.EntityFrameworkCore;
using SolidarSYS.Application.Dashboard.DTOs;
using SolidarSYS.Application.Dashboard.Interfaces;
using SolidarSYS.Infrastructure.Data;

namespace SolidarSYS.Infrastructure.Repositories;

public class DashboardRepository(SolidarSysDbContext db) : IDashboardRepository
{
    public Task<int> ContarBeneficiariosAtivosAsync(CancellationToken ct = default) =>
        db.Beneficiarios.CountAsync(b => b.Ativo, ct);

    public Task<int> ContarDoadoresAtivosAsync(CancellationToken ct = default) =>
        db.Doadores.CountAsync(d => d.Ativo, ct);

    public Task<int> ContarComunidadesAtivasAsync(CancellationToken ct = default) =>
        db.Comunidades.CountAsync(c => c.Ativa, ct);

    public Task<int> ContarVisitasNoPeriodoAsync(DateOnly inicio, DateOnly fim, CancellationToken ct = default) =>
        db.Visitas.CountAsync(v => v.DataVisita >= inicio && v.DataVisita <= fim, ct);

    public async Task<IList<DateTime>> ListarDatasCadastroBeneficiariosAsync(DateTime desdeUtc, CancellationToken ct = default) =>
        await db.Beneficiarios
            .Where(b => b.Ativo && b.DataCriacao >= desdeUtc)
            .Select(b => b.DataCriacao)
            .ToListAsync(ct);

    public async Task<IList<DateOnly>> ListarDatasCadastroDoadoresAsync(DateOnly desde, CancellationToken ct = default) =>
        await db.Doadores
            .Where(d => d.Ativo && d.DataCadastro >= desde)
            .Select(d => d.DataCadastro)
            .ToListAsync(ct);

    public async Task<IList<ComunidadeResumoDto>> ListarBeneficiariosPorComunidadeAsync(CancellationToken ct = default)
    {
        var dados = await db.Comunidades
            .Where(c => c.Ativa)
            .OrderBy(c => c.Nome)
            .Select(c => new
            {
                c.Nome,
                Total = db.Beneficiarios.Count(b => b.IdComunidade == c.IdComunidade && b.Ativo),
            })
            .ToListAsync(ct);

        return dados.Select(x => new ComunidadeResumoDto(x.Nome, x.Total)).ToList();
    }
}
