using SolidarSYS.Application.Beneficiarios.DTOs;
using SolidarSYS.Application.Beneficiarios.Interfaces;
using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Beneficiarios.Services;

public class BeneficiarioService(IBeneficiarioRepository repo) : IBeneficiarioService
{
    public async Task<PagedResultDto<BeneficiarioListItemDto>> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default)
    {
        var (items, total) = await repo.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);
        return new PagedResultDto<BeneficiarioListItemDto>(
            items.Select(Mapear).ToList(), total, pagina, tamanhoPagina);
    }

    public async Task<BeneficiarioListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var beneficiario = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Beneficiário não encontrado.");
        return Mapear(beneficiario);
    }

    public async Task<BeneficiarioListItemDto> CriarAsync(CriarBeneficiarioDto dto, CancellationToken ct = default)
    {
        var cpf = SomenteDigitos(dto.Cpf);
        var comunidade = await ObterComunidadeAtivaAsync(dto.IdComunidade, ct);

        if (await repo.ExisteCpfAsync(cpf, null, ct))
            throw new InvalidOperationException("Já existe um beneficiário cadastrado com este CPF.");

        var beneficiario = new Beneficiario
        {
            Nome = dto.Nome.Trim(),
            Cpf = cpf,
            DataNascimento = dto.DataNascimento,
            Endereco = dto.Endereco.Trim(),
            IdComunidade = comunidade.IdComunidade,
            Comunidade = comunidade,
            TotalMembrosFamilia = 0,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
        };

        await repo.AdicionarAsync(beneficiario, ct);
        await repo.SalvarAlteracoesAsync(ct);
        await repo.AtualizarTotalBeneficiariosAsync(comunidade.IdComunidade, ct);

        return Mapear(beneficiario);
    }

    public async Task<BeneficiarioListItemDto> AtualizarAsync(int id, AtualizarBeneficiarioDto dto, CancellationToken ct = default)
    {
        var beneficiario = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Beneficiário não encontrado.");

        var cpf = SomenteDigitos(dto.Cpf);
        var idComunidadeAnterior = beneficiario.IdComunidade;

        // Permite manter a comunidade atual mesmo que ela tenha sido inativada depois.
        var comunidade = dto.IdComunidade == idComunidadeAnterior
            ? await repo.ObterComunidadeAsync(dto.IdComunidade, ct)
                ?? throw new KeyNotFoundException("Comunidade não encontrada.")
            : await ObterComunidadeAtivaAsync(dto.IdComunidade, ct);

        if (await repo.ExisteCpfAsync(cpf, id, ct))
            throw new InvalidOperationException("Já existe um beneficiário cadastrado com este CPF.");

        beneficiario.Nome = dto.Nome.Trim();
        beneficiario.Cpf = cpf;
        beneficiario.DataNascimento = dto.DataNascimento;
        beneficiario.Endereco = dto.Endereco.Trim();
        beneficiario.IdComunidade = comunidade.IdComunidade;
        beneficiario.Comunidade = comunidade;

        await repo.SalvarAlteracoesAsync(ct);

        await repo.AtualizarTotalBeneficiariosAsync(comunidade.IdComunidade, ct);
        if (idComunidadeAnterior != comunidade.IdComunidade)
            await repo.AtualizarTotalBeneficiariosAsync(idComunidadeAnterior, ct);

        return Mapear(beneficiario);
    }

    public async Task InativarAsync(int id, CancellationToken ct = default)
    {
        var beneficiario = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Beneficiário não encontrado.");

        if (!beneficiario.Ativo)
            return;

        beneficiario.Ativo = false;
        await repo.SalvarAlteracoesAsync(ct);
        await repo.AtualizarTotalBeneficiariosAsync(beneficiario.IdComunidade, ct);
    }

    public async Task<IList<ComunidadeOpcaoDto>> ListarComunidadesAsync(CancellationToken ct = default)
    {
        var comunidades = await repo.ListarComunidadesAtivasAsync(ct);
        return comunidades.Select(c => new ComunidadeOpcaoDto(c.IdComunidade, c.Nome)).ToList();
    }

    private async Task<Comunidade> ObterComunidadeAtivaAsync(int idComunidade, CancellationToken ct)
    {
        var comunidade = await repo.ObterComunidadeAsync(idComunidade, ct)
            ?? throw new KeyNotFoundException("Comunidade não encontrada.");

        if (!comunidade.Ativa)
            throw new InvalidOperationException("A comunidade selecionada está inativa.");

        return comunidade;
    }

    private static string SomenteDigitos(string valor) =>
        new(valor.Where(char.IsDigit).ToArray());

    private static BeneficiarioListItemDto Mapear(Beneficiario b) =>
        new(b.IdBeneficiario, b.Nome, b.Cpf, b.DataNascimento, b.Endereco,
            b.IdComunidade, b.Comunidade?.Nome ?? string.Empty,
            b.TotalMembrosFamilia, b.Ativo, b.DataCriacao);
}