using SolidarSYS.Application.Setores.DTOs;
using SolidarSYS.Application.Setores.Interfaces;
using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Setores.Services;

public class SetoresService(ISetoresRepository repo) : ISetoresService
{
    public async Task<PagedResultDto<SetoresListItemDto>> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default)
    {
        var (items, total) = await repo.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);

        return new PagedResultDto<SetoresListItemDto>(
            items.Select(Mapear).ToList(), total, pagina, tamanhoPagina);
    }

    public async Task<SetoresListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var setor = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Setor não encontrada.");

        return Mapear(setor);
    }

    public async Task<SetoresListItemDto> BuscarPorNomeAsync(string nome, CancellationToken ct = default)
    {
        var setor = await repo.BuscarPorNomeAsync(nome, ct)
            ?? throw new KeyNotFoundException("Setor não encontrada.");

        return Mapear(setor);
    }

    public async Task<SetoresListItemDto> CriarAsync(CriarSetoresDto dto, CancellationToken ct = default)
    {
        if (await repo.BuscarPorNomeAsync(dto.Nome, ct) is not null)
            throw new InvalidOperationException("Já existe um setor cadastrado com este nome");

        var setores = new Setor
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        await repo.CriarAsync(setores, ct);
        await repo.SalvarAlteracoesAsync(ct);

        return Mapear(setores);
    }

    public async Task<SetoresListItemDto> AtualizarAsync(int id, AtualizarSetoresDto dto, CancellationToken ct = default)
    {
        var setor = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Setor não encontrado.");

        setor.Nome = dto.Nome;
        setor.Descricao = dto.Descricao;

        await repo.AtualizarAsync(setor, ct);
        await repo.SalvarAlteracoesAsync(ct);

        return Mapear(setor);
    }

    public async Task AlternarAtivoAsync(int id, CancellationToken ct = default)
    {
        var setor = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Setor não encontrado.");

        setor.Ativo = !setor.Ativo;
        await repo.AtualizarAsync(setor, ct);
        await repo.SalvarAlteracoesAsync(ct);
    }

    private static SetoresListItemDto Mapear(Setor c) =>
        new(c.Id, c.Ativo, c.Nome, c.Descricao,c.DataCriacao);
}