namespace SolidarSYS.Domain.Entities;

public class SetorPermissao
{
    public int IdSetor { get; set; }
    public int IdPermissao { get; set; }

    public Setor Setor { get; set; } = null!;
    public Permissao Permissao { get; set; } = null!;
}
