namespace SolidarSYS.Domain.Entities;

public class Visita
{
    public int IdVisita { get; set; }
    public int IdBeneficiario { get; set; }
    public DateOnly DataVisita { get; set; }
    public string Responsavel { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public Beneficiario? Beneficiario { get; set; }
}
