namespace SolidarSYS.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public bool Revogado { get; set; } = false;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;

    public bool EstaValido() => !Revogado && ExpiraEm > DateTime.UtcNow;
}
