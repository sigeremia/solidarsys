namespace SolidarSYS.Domain.Entities;

public class Permissao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Recurso { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;

    public ICollection<SetorPermissao> SetorPermissoes { get; set; } = [];
}
