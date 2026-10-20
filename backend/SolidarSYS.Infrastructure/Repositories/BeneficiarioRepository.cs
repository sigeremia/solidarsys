using Microsoft.EntityFrameworkCore;
using SolidarSYS.Application.Beneficiarios.Interfaces;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Infrastructure.Data;

namespace SolidarSYS.Infrastructure.Repositories;

public class BeneficiarioRepository(SolidarSysDbContext db) : IBeneficiarioRepository
{
    public async Task<(IList<Beneficiario> Items, int Total)> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var query = db.Beneficiarios.Include(b => b.Comunidade).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            var digitos = new string(termo.Where(char.IsDigit).ToArray());
            var temDigitos = digitos.Length > 0;

            query = query.Where(b =>
                b.Nome.ToLower().Contains(termo)
                || b.Comunidade!.Nome.ToLower().Contains(termo)
                || (temDigitos && b.Cpf.Contains(digitos)));
        }

        if (ativo.HasValue)
            query = query.Where(b => b.Ativo == ativo.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(b => b.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Beneficiario?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        db.Beneficiarios.Include(b => b.Comunidade)
            .FirstOrDefaultAsync(b => b.IdBeneficiario == id, ct);

    public Task<bool> ExisteCpfAsync(string cpf, int? excluirId = null, CancellationToken ct = default) =>
        db.Beneficiarios.AnyAsync(b => b.Cpf == cpf && b.IdBeneficiario != excluirId, ct);

    public async Task<Beneficiario> AdicionarAsync(Beneficiario beneficiario, CancellationToken ct = default)
    {
        await db.Beneficiarios.AddAsync(beneficiario, ct);
        return beneficiario;
    }

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);

    public Task<Comunidade?> ObterComunidadeAsync(int idComunidade, CancellationToken ct = default) =>
        db.Comunidades.FirstOrDefaultAsync(c => c.IdComunidade == idComunidade, ct);

    public async Task<IList<Comunidade>> ListarComunidadesAtivasAsync(CancellationToken ct = default) =>
        await db.Comunidades.Where(c => c.Ativa).OrderBy(c => c.Nome).ToListAsync(ct);

    public async Task AtualizarTotalBeneficiariosAsync(int idComunidade, CancellationToken ct = default)
    {
        var comunidade = await db.Comunidades.FirstOrDefaultAsync(c => c.IdComunidade == idComunidade, ct);
        if (comunidade is null)
            return;

        comunidade.TotalBeneficiarios = await db.Beneficiarios
            .CountAsync(b => b.IdComunidade == idComunidade && b.Ativo, ct);

        await db.SaveChangesAsync(ct);
    }
}