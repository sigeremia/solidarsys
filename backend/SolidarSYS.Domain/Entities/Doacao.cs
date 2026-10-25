using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Domain.Entities;

public class Doacao
{
    public int Id { get; set; }
    public int IdDoador { get; set; }
    public DateOnly Data { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public TipoDoacao Tipo { get; set; }
    public DateTimeOffset DataCadastro { get; set; } = DateTimeOffset.UtcNow;

    public Doador? Doador { get; set; }
}