using Microsoft.EntityFrameworkCore;
using SolidarSYS.Application.Visitas.Interfaces;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Infrastructure.Data;

namespace SolidarSYS.Infrastructure.Repositories;

public class VisitaRepository(SolidarSysDbContext db) : IVisitaRepository
{
    public async Task<(IList<Visita> Items, int Total)> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, CancellationToken ct = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var query = db.Visitas.Include(v => v.Beneficiario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(v =>
                v.Beneficiario != null && v.Beneficiario.Nome.Contains(termo)
                || v.Responsavel.Contains(termo)
                || (v.Observacoes != null && v.Observacoes.Contains(termo)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(v => v.DataVisita)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Visita?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        db.Visitas.Include(v => v.Beneficiario)
            .FirstOrDefaultAsync(v => v.IdVisita == id, ct);

    public async Task<Visita> AdicionarAsync(Visita visita, CancellationToken ct = default)
    {
        await db.Visitas.AddAsync(visita, ct);
        return visita;
    }

    public Task AtualizarAsync(Visita visita, CancellationToken ct = default)
    {
        db.Visitas.Update(visita);
        return Task.CompletedTask;
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var visita = await db.Visitas.FirstOrDefaultAsync(v => v.IdVisita == id, ct);
        if (visita is null)
            return;

        db.Visitas.Remove(visita);
        await db.SaveChangesAsync(ct);
    }

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);

    public Task<bool> ExisteVisitaNoDiaAsync(int idBeneficiario, DateOnly dataVisita, int? excluirId = null, CancellationToken ct = default) =>
        db.Visitas.AnyAsync(v => v.IdBeneficiario == idBeneficiario && v.DataVisita == dataVisita && v.IdVisita != (excluirId ?? 0), ct);

    public async Task<IList<Beneficiario>> ListarBeneficiariosAtivosAsync(CancellationToken ct = default) =>
        await db.Beneficiarios.Where(b => b.Ativo).OrderBy(b => b.Nome).ToListAsync(ct);

    public Task<Beneficiario?> ObterBeneficiarioAsync(int idBeneficiario, CancellationToken ct = default) =>
        db.Beneficiarios.FirstOrDefaultAsync(b => b.IdBeneficiario == idBeneficiario, ct);
}
