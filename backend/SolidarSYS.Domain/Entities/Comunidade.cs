namespace SolidarSYS.Domain.Entities;

public class Comunidade
{
    public int IdComunidade { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Regiao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public bool Ativa { get; set; } = true;
    public int TotalBeneficiarios { get; set; }
}