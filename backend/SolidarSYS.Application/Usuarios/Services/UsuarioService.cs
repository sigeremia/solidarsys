using SolidarSYS.Application.Auth.Interfaces;
using SolidarSYS.Application.Usuarios.DTOs;
using SolidarSYS.Application.Usuarios.Interfaces;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Application.Usuarios.Services;

public class UsuarioService(IUsuarioRepository repo, IPasswordHasher passwordHasher) : IUsuarioService
{
    public async Task<PagedResultDto<UsuarioListItemDto>> ListarAsync(
        int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct)
    {
        var (items, total) = await repo.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);
        return new PagedResultDto<UsuarioListItemDto>(
            items.Select(Mapear).ToList(), total, pagina, tamanhoPagina);
    }

    public async Task<UsuarioListItemDto> ObterPorIdAsync(int id, CancellationToken ct)
    {
        var usuario = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");
        return Mapear(usuario);
    }

    public async Task<UsuarioListItemDto> CriarAsync(CriarUsuarioDto dto, CancellationToken ct)
    {
        if (await repo.ExisteEmailAsync(dto.Email, null, ct))
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");

        if (await repo.ExisteCpfAsync(dto.Cpf, null, ct))
            throw new InvalidOperationException("Já existe um usuário com este CPF.");

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email.ToLower(),
            Senha = passwordHasher.Hash(dto.Senha),
            DataNascimento = dto.DataNascimento,
            NivelAcesso = dto.NivelAcesso,
            IdSetor = dto.IdSetor,
            Cpf = dto.Cpf,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
        };

        await repo.AdicionarAsync(usuario, ct);
        await repo.SalvarAlteracoesAsync(ct);

        return await ObterPorIdAsync(usuario.IdUsuario, ct);
    }

    public async Task<UsuarioListItemDto> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct)
    {
        var usuario = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        usuario.Nome = dto.Nome;
        usuario.DataNascimento = dto.DataNascimento;
        usuario.NivelAcesso = dto.NivelAcesso;
        usuario.IdSetor = dto.IdSetor;

        await repo.AtualizarAsync(usuario, ct);
        await repo.SalvarAlteracoesAsync(ct);

        return Mapear(usuario);
    }

    public async Task AlternarAtivoAsync(int id, CancellationToken ct)
    {
        var usuario = await repo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        usuario.Ativo = !usuario.Ativo;
        await repo.AtualizarAsync(usuario, ct);
        await repo.SalvarAlteracoesAsync(ct);
    }

    private static UsuarioListItemDto Mapear(Usuario u) =>
        new(u.IdUsuario, u.Nome, u.Email, u.DataNascimento, u.NivelAcesso, u.Setor?.Nome, u.Ativo, u.DataCriacao);
}