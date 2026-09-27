using Microsoft.EntityFrameworkCore;
using SolidarSYS.Application.Comunidades.Interfaces;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Infrastructure.Data;

namespace SolidarSYS.Infrastructure.Repositories;

public class ComunidadeRepository(SolidarSysDbContext db) : IComunidadeRepository
{
    public async Task<(IList<Comunidade> Items, int Total)> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativa, CancellationToken ct = default)
    {
        var query = db.Comunidades.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.ToLower();
            query = query.Where(c => c.Nome.ToLower().Contains(termo)
                || c.Regiao.ToLower().Contains(termo)
                || c.Descricao.ToLower().Contains(termo));
        }

        if (ativa.HasValue)
            query = query.Where(c => c.Ativa == ativa.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Comunidade?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        db.Comunidades.FirstOrDefaultAsync(c => c.IdComunidade == id, ct);

    public async Task<Comunidade?> BuscarPorNomeAsync(string nome, CancellationToken ct = default)
    {
        var termo = nome.ToLower();
        return await db.Comunidades
            .FirstOrDefaultAsync(c => c.Nome.ToLower() == termo, ct);
    }

    public async Task<Comunidade> CriarAsync(Comunidade comunidade, CancellationToken ct = default)
    {
        await db.Comunidades.AddAsync(comunidade, ct);
        return comunidade;
    }

    public Task AtualizarAsync(Comunidade comunidade, CancellationToken ct = default)
    {
        db.Comunidades.Update(comunidade);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);

    public async Task AlternarAtivoAsync(int id, CancellationToken ct = default)
    {
        var comunidade = await db.Comunidades.FindAsync(id, ct);
        if (comunidade is null)
            return;

        comunidade.Ativa = !comunidade.Ativa;
        await db.SaveChangesAsync(ct);
    }
}