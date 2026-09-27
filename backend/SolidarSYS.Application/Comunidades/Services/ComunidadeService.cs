using SolidarSYS.Application.Comunidades.DTOs;
using SolidarSYS.Application.Comunidades.Interfaces;
using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Comunidades.Services;

public class ComunidadeService(IComunidadeRepository repo) : IComunidadeService
{
    public async Task<PagedResultDto<ComunidadeListItemDto>> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default)
    {
        var (items, total) = await repo.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);
        // TODO: Para cada comunidade desta página, contar os Beneficiarios vinculados ao IdComunidade, atualizar TotalBeneficiarios no registro e persistir antes de montar a resposta.
        return new PagedResultDto<ComunidadeListItemDto>(
            items.Select(Mapear).ToList(), total, pagina, tamanhoPagina);
    }

    public async Task<ComunidadeListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var comunidade = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Comunidade não encontrada.");

        return Mapear(comunidade);
    }

    public async Task<ComunidadeListItemDto> BuscarPorNomeAsync(string nome, CancellationToken ct = default)
    {
        var comunidade = await repo.BuscarPorNomeAsync(nome, ct)
            ?? throw new KeyNotFoundException("Comunidade não encontrada.");

        return Mapear(comunidade);
    }

    public async Task<ComunidadeListItemDto> CriarAsync(CriarComunidadeDto dto, CancellationToken ct = default)
    {
        if (await repo.BuscarPorNomeAsync(dto.Nome, ct) is not null)
            throw new InvalidOperationException("Já existe uma comunidade cadastrada com este nome");

        var comunidade = new Comunidade
        {
            Nome = dto.Nome,
            Regiao = dto.Regiao,
            Descricao = dto.Descricao,
            Ativa = true,
            DataCriacao = DateTime.UtcNow,
            TotalBeneficiarios = 0
        };

        await repo.CriarAsync(comunidade, ct);
        await repo.SalvarAlteracoesAsync(ct);

        return Mapear(comunidade);
    }

    public async Task<ComunidadeListItemDto> AtualizarAsync(int id, AtualizarComunidadeDto dto, CancellationToken ct = default)
    {
        var comunidade = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Comunidade não encontrada.");

        comunidade.Nome = dto.Nome;
        comunidade.Regiao = dto.Regiao;
        comunidade.Descricao = dto.Descricao;

        await repo.AtualizarAsync(comunidade, ct);
        await repo.SalvarAlteracoesAsync(ct);

        return Mapear(comunidade);
    }

    public async Task AlternarAtivoAsync(int id, CancellationToken ct = default)
    {
        var comunidade = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Comunidade não encontrada.");

        comunidade.Ativa = !comunidade.Ativa;
        await repo.AtualizarAsync(comunidade, ct);
        await repo.SalvarAlteracoesAsync(ct);
    }

    private static ComunidadeListItemDto Mapear(Comunidade c) =>
        new(c.IdComunidade, c.Ativa, c.Nome, c.Regiao, c.Descricao,c.DataCriacao, c.TotalBeneficiarios);
}