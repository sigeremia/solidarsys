using SolidarSYS.Application.Auth.Interfaces;

namespace SolidarSYS.Infrastructure.Auth;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string senha) =>
        BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);

    public bool Verificar(string senha, string hash) =>
        BCrypt.Net.BCrypt.Verify(senha, hash);
}
