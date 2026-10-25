using SolidarSYS.Application.Beneficiarios.DTOs;
using SolidarSYS.Application.Doadores.DTOs;
using SolidarSYS.Application.Doadores.Interfaces;
using SolidarSYS.Application.Doadores.Validators;
using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Doadores.Services;

public sealed class DoadorService(IDoadorRepository repo) : IDoadorService
{
    public async Task<PagedResultDto<DoadorListItemDto>> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default)
    {
        var (items, total) = await repo.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);
        return new PagedResultDto<DoadorListItemDto>(items.Select(Mapear).ToList(), total, pagina, tamanhoPagina);
    }

    public async Task<DoadorListItemDto> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var doador = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Doador não encontrado.");
        return Mapear(doador);
    }

    public async Task<DoadorListItemDto> CriarAsync(CriarDoadorDto dto, CancellationToken ct = default)
    {
        var documento = DoadorDocumento.Normalizar(dto.Documento);
        if (await repo.ExisteDocumentoAsync(documento, null, ct))
            throw new InvalidOperationException("Já existe um doador cadastrado com este documento.");

        var doador = new Doador
        {
            Tipo = dto.Tipo,
            Nome = dto.Nome.Trim(),
            NomeFantasia = TextoOpcional(dto.NomeFantasia),
            Documento = documento,
            Telefone = NormalizarTelefone(dto.Telefone),
            Email = TextoOpcional(dto.Email),
            Observacao = TextoOpcional(dto.Observacao),
            DataCadastro = DateOnly.FromDateTime(DateTime.UtcNow),
            Ativo = true,
        };

        await repo.AdicionarAsync(doador, ct);
        await repo.SalvarAlteracoesAsync(ct);
        return Mapear(doador);
    }

    public async Task<DoadorListItemDto> AtualizarAsync(int id, AtualizarDoadorDto dto, CancellationToken ct = default)
    {
        var doador = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Doador não encontrado.");
        var documento = DoadorDocumento.Normalizar(dto.Documento);
        if (await repo.ExisteDocumentoAsync(documento, id, ct))
            throw new InvalidOperationException("Já existe um doador cadastrado com este documento.");

        doador.Tipo = dto.Tipo;
        doador.Nome = dto.Nome.Trim();
        doador.NomeFantasia = TextoOpcional(dto.NomeFantasia);
        doador.Documento = documento;
        doador.Telefone = NormalizarTelefone(dto.Telefone);
        doador.Email = TextoOpcional(dto.Email);
        doador.Observacao = TextoOpcional(dto.Observacao);

        await repo.SalvarAlteracoesAsync(ct);
        return Mapear(doador);
    }

    public async Task InativarAsync(int id, CancellationToken ct = default)
    {
        var doador = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Doador não encontrado.");
        if (!doador.Ativo)
            return;

        doador.Ativo = false;
        await repo.SalvarAlteracoesAsync(ct);
    }

    private static string? TextoOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string? NormalizarTelefone(string? telefone) =>
        string.IsNullOrWhiteSpace(telefone) ? null : new string(telefone.Where(char.IsDigit).ToArray());

    private static DoadorListItemDto Mapear(Doador doador) =>
        new(doador.IdDoador, doador.Tipo, doador.Nome, doador.NomeFantasia, doador.Documento,
            doador.Telefone, doador.Email, doador.DataCadastro, doador.Observacao, doador.Ativo,
            doador.Doacoes.OrderByDescending(d => d.Data).Select(d => (DateOnly?)d.Data).FirstOrDefault());
}