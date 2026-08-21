namespace SolidarSYS.Domain.Entities;

public class Setor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<Usuario> Usuarios { get; set; } = [];
    public ICollection<SetorPermissao> SetorPermissoes { get; set; } = [];
}
