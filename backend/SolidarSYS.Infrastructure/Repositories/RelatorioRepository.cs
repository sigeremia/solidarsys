using Microsoft.EntityFrameworkCore;
using SolidarSYS.Application.Relatorios.DTOs;
using SolidarSYS.Application.Relatorios.Interfaces;
using SolidarSYS.Infrastructure.Data;

namespace SolidarSYS.Infrastructure.Repositories;

public class RelatorioRepository(SolidarSysDbContext db) : IRelatorioRepository
{
    public Task<int> ContarBeneficiariosAtivosAsync(CancellationToken ct = default) =>
        db.Beneficiarios.CountAsync(b => b.Ativo, ct);

    public Task<int> ContarComunidadesAtendidasAsync(CancellationToken ct = default) =>
        db.Comunidades.CountAsync(
            c => c.Ativa && db.Beneficiarios.Any(b => b.IdComunidade == c.IdComunidade && b.Ativo),
            ct);

    public async Task<IList<DateTime>> ListarDatasCadastroBeneficiariosAsync(
        DateTime desdeUtc, DateTime ateUtcExclusivo, CancellationToken ct = default) =>
        await db.Beneficiarios
            .Where(b => b.Ativo && b.DataCriacao >= desdeUtc && b.DataCriacao < ateUtcExclusivo)
            .Select(b => b.DataCriacao)
            .ToListAsync(ct);

    public async Task<IList<DoacaoPeriodoDto>> ListarDoacoesNoPeriodoAsync(
        DateOnly inicio, DateOnly fim, CancellationToken ct = default)
    {
        var dados = await db.Doacoes
            .Where(d => d.Data >= inicio && d.Data <= fim)
            .Select(d => new { d.Data, d.Valor })
            .ToListAsync(ct);

        return dados.Select(x => new DoacaoPeriodoDto(x.Data, x.Valor)).ToList();
    }

    public async Task<IList<RelatorioComunidadeDto>> ListarBeneficiariosPorComunidadeAsync(CancellationToken ct = default)
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

        return dados
            .Where(x => x.Total > 0)
            .Select(x => new RelatorioComunidadeDto(x.Nome, x.Total))
            .ToList();
    }
}
