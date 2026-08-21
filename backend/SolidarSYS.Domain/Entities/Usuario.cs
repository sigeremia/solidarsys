using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    public NivelAcesso NivelAcesso { get; set; }
    public int? IdSetor { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataUltimoAcesso { get; set; }

    // Controle de bloqueio por tentativas falhas
    public short TentativasFalhas { get; set; } = 0;
    public DateTime? BloqueadoAte { get; set; }

    public Setor? Setor { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<TokenRecuperacaoSenha> TokensRecuperacao { get; set; } = [];

    public bool EstaBloqueado() =>
        BloqueadoAte.HasValue && BloqueadoAte.Value > DateTime.UtcNow;

    public void RegistrarTentativaFalha()
    {
        TentativasFalhas++;
        if (TentativasFalhas >= 5)
            BloqueadoAte = DateTime.UtcNow.AddMinutes(15);
    }

    public void ResetarTentativas()
    {
        TentativasFalhas = 0;
        BloqueadoAte = null;
    }
}
