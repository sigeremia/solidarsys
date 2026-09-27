using Microsoft.EntityFrameworkCore;
using SolidarSYS.Application.Setores.Interfaces;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Infrastructure.Data;

namespace SolidarSYS.Infrastructure.Setores;

public class SetoresRepository(SolidarSysDbContext db) : ISetoresRepository
{
    public async Task<(IList<Setor> Items, int Total)> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default)
    {
        var query = db.Setores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.ToLower();
            query = query.Where(c => c.Nome.ToLower().Contains(termo)
                || c.Descricao.ToLower().Contains(termo));
        }

        if (ativo.HasValue)
            query = query.Where(c => c.Ativo == ativo.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Setor?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        db.Setores.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Setor?> BuscarPorNomeAsync(string nome, CancellationToken ct = default)
    {
        var termo = nome.ToLower();
        return await db.Setores
            .FirstOrDefaultAsync(c => c.Nome.ToLower() == termo, ct);
    }

    public async Task<Setor> CriarAsync(Setor setor, CancellationToken ct = default)
    {
        await db.Setores.AddAsync(setor, ct);
        return setor;
    }

    public Task AtualizarAsync(Setor setor, CancellationToken ct = default)
    {
        db.Setores.Update(setor);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);

    public async Task AlternarAtivoAsync(int id, CancellationToken ct = default)
    {
        var setores = await db.Setores.FindAsync(id, ct);
        if (setores is null)
            return;

        setores.Ativo = !setores.Ativo;
        await db.SaveChangesAsync(ct);
    }
}