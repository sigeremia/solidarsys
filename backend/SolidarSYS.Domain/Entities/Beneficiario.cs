namespace SolidarSYS.Domain.Entities;

public class Beneficiario
{
    public int IdBeneficiario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public int IdComunidade { get; set; }
    public int TotalMembrosFamilia { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Comunidade? Comunidade { get; set; }
}