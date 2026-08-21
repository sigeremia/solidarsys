namespace SolidarSYS.Domain.Entities;

public class TokenRecuperacaoSenha
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Token { get; set; } = string.Empty; // SHA-256 do token enviado por e-mail
    public DateTime ExpiraEm { get; set; }
    public bool Usado { get; set; } = false;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;

    public bool EstaValido() => !Usado && ExpiraEm > DateTime.UtcNow;
}
