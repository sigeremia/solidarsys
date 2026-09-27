using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SolidarSYS.Domain.Enums;

namespace SolidarSYS.Infrastructure.Data;

public sealed class SolidarSysDbContextFactory : IDesignTimeDbContextFactory<SolidarSysDbContext>
{
    public SolidarSysDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SolidarSysDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=solidarsys;Username=postgres;Password=postgres",
                npgsql => npgsql.MapEnum<NivelAcesso>("nivel_acesso"))
            .Options;

        return new SolidarSysDbContext(options);
    }
}