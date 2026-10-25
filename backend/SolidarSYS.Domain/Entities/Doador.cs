using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Domain.Entities;

public class Doador
{
    public int IdDoador { get; set; }
    public TipoDoador Tipo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateOnly DataCadastro { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string? Observacao { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Doacao> Doacoes { get; set; } = new List<Doacao>();
}