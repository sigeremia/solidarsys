using SolidarSYS.Application.Visitas.DTOs;
using SolidarSYS.Application.Visitas.Interfaces;
using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Visitas.Services;

public class VisitaService(IVisitaRepository repo) : IVisitaService
{
    public async Task<PagedResultDto<VisitaListItemDto>> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, CancellationToken ct = default)
    {
        var (items, total) = await repo.ListarAsync(pagina, tamanhoPagina, busca, ct);
        return new PagedResultDto<VisitaListItemDto>(
            items.Select(Mapear).ToList(),
            total,
            pagina,
            tamanhoPagina);
    }

    public async Task<VisitaListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var visita = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Visita não encontrada.");

        return Mapear(visita);
    }

    public async Task<IList<BeneficiarioOpcaoDto>> ListarBeneficiariosAsync(CancellationToken ct = default)
    {
        var beneficiarios = await repo.ListarBeneficiariosAtivosAsync(ct);
        return beneficiarios
            .OrderBy(b => b.Nome)
            .Select(b => new BeneficiarioOpcaoDto(b.IdBeneficiario, b.Nome))
            .ToList();
    }

    public async Task<VisitaListItemDto> CriarAsync(CriarVisitaDto dto, CancellationToken ct = default)
    {
        var beneficiario = await repo.ObterBeneficiarioAsync(dto.IdBeneficiario, ct)
            ?? throw new KeyNotFoundException("Beneficiário não encontrado.");

        if (!beneficiario.Ativo)
            throw new InvalidOperationException("O beneficiário selecionado está inativo.");

        if (await repo.ExisteVisitaNoDiaAsync(dto.IdBeneficiario, dto.DataVisita, null, ct))
            throw new InvalidOperationException("Já existe uma visita cadastrada para este beneficiário na data informada.");

        var visita = new Visita
        {
            IdBeneficiario = dto.IdBeneficiario,
            Beneficiario = beneficiario,
            DataVisita = dto.DataVisita,
            Responsavel = dto.Responsavel.Trim(),
            Observacoes = string.IsNullOrWhiteSpace(dto.Observacoes) ? null : dto.Observacoes.Trim(),
            DataCadastro = DateTime.UtcNow,
        };

        await repo.AdicionarAsync(visita, ct);
        await repo.SalvarAlteracoesAsync(ct);

        return Mapear(visita);
    }

    public async Task<VisitaListItemDto> AtualizarAsync(int id, AtualizarVisitaDto dto, CancellationToken ct = default)
    {
        var visita = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Visita não encontrada.");

        var beneficiario = await repo.ObterBeneficiarioAsync(dto.IdBeneficiario, ct)
            ?? throw new KeyNotFoundException("Beneficiário não encontrado.");

        if (!beneficiario.Ativo)
            throw new InvalidOperationException("O beneficiário selecionado está inativo.");

        if (await repo.ExisteVisitaNoDiaAsync(dto.IdBeneficiario, dto.DataVisita, id, ct))
            throw new InvalidOperationException("Já existe uma visita cadastrada para este beneficiário na data informada.");

        visita.IdBeneficiario = dto.IdBeneficiario;
        visita.Beneficiario = beneficiario;
        visita.DataVisita = dto.DataVisita;
        visita.Responsavel = dto.Responsavel.Trim();
        visita.Observacoes = string.IsNullOrWhiteSpace(dto.Observacoes) ? null : dto.Observacoes.Trim();

        await repo.AtualizarAsync(visita, ct);
        await repo.SalvarAlteracoesAsync(ct);

        return Mapear(visita);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var visita = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Visita não encontrada.");

        await repo.ExcluirAsync(visita.IdVisita, ct);
    }

    private static VisitaListItemDto Mapear(Visita visita) =>
        new(
            visita.IdVisita,
            visita.IdBeneficiario,
            visita.Beneficiario?.Nome ?? string.Empty,
            visita.DataVisita,
            visita.Responsavel,
            visita.Observacoes,
            visita.DataCadastro);
}
