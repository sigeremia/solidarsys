using SolidarSYS.Domain.Entities;

namespace SolidarSYS.Application.Auth.Interfaces;

public interface IUsuarioRepository
{
    // Auth
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken ct = default);
    Task<RefreshToken?> ObterRefreshTokenAsync(string tokenHash, CancellationToken ct = default);
    Task AdicionarRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    Task<TokenRecuperacaoSenha?> ObterTokenRecuperacaoAsync(string tokenHash, CancellationToken ct = default);
    Task AdicionarTokenRecuperacaoAsync(TokenRecuperacaoSenha token, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);

    // CRUD usuários
    Task<(IList<Usuario> Items, int Total)> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null, CancellationToken ct = default);
    Task<bool> ExisteCpfAsync(string cpf, int? excluirId = null, CancellationToken ct = default);
    Task<Usuario> AdicionarAsync(Usuario usuario, CancellationToken ct = default);
}
