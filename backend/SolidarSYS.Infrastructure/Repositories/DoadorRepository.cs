using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SolidarSYS.Application.Doadores.Interfaces;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Infrastructure.Data;

namespace SolidarSYS.Infrastructure.Repositories;

public sealed class DoadorRepository(SolidarSysDbContext db) : IDoadorRepository
{
    public async Task<(IList<Doador> Items, int Total)> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var query = db.Doadores
            .Include(d => d.Doacoes.OrderByDescending(doacao => doacao.Data).Take(1))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            var digitos = new string(termo.Where(char.IsDigit).ToArray());
            query = query.Where(d =>
                d.Nome.ToLower().Contains(termo)
                || (d.NomeFantasia != null && d.NomeFantasia.ToLower().Contains(termo))
                || d.Documento.ToLower().Contains(termo)
                || (digitos.Length > 0 && d.Documento.Contains(digitos)));
        }

        if (ativo.HasValue)
            query = query.Where(d => d.Ativo == ativo.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(d => d.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Doador?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        db.Doadores
            .Include(d => d.Doacoes.OrderByDescending(doacao => doacao.Data).Take(1))
            .FirstOrDefaultAsync(d => d.IdDoador == id, ct);

    public Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken ct = default)
    {
        var query = db.Doadores.Where(d => d.Documento == documento);
        if (excluirId.HasValue)
            query = query.Where(d => d.IdDoador != excluirId.Value);
        return query.AnyAsync(ct);
    }

    public async Task AdicionarAsync(Doador doador, CancellationToken ct = default) =>
        await db.Doadores.AddAsync(doador, ct);

    public async Task SalvarAlteracoesAsync(CancellationToken ct = default)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (EhDocumentoDuplicado(ex))
        {
            throw new InvalidOperationException("Já existe um doador cadastrado com este documento.", ex);
        }
    }

    private static bool EhDocumentoDuplicado(DbUpdateException exception)
    {
        if (exception.GetBaseException() is PostgresException postgres)
            return postgres.SqlState == PostgresErrorCodes.UniqueViolation
                && postgres.ConstraintName is "IX_doadores_documento" or "uq_doadores_documento";

        return exception.GetBaseException() is SqliteException sqlite
            && sqlite.SqliteErrorCode == 19
            && sqlite.Message.Contains("doadores.documento", StringComparison.OrdinalIgnoreCase);
    }
}