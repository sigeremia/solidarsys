using FluentAssertions;
using Moq;
using SolidarSYS.Application.Dashboard.DTOs;
using SolidarSYS.Application.Dashboard.Interfaces;
using SolidarSYS.Application.Dashboard.Services;
using Xunit;

namespace SolidarSYS.Tests;

public class DashboardServiceTests
{
    private static Mock<IDashboardRepository> CriarRepo()
    {
        var repo = new Mock<IDashboardRepository>();
        var agora = DateTime.UtcNow;

        repo.Setup(r => r.ContarBeneficiariosAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(12);
        repo.Setup(r => r.ContarDoadoresAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);
        repo.Setup(r => r.ContarComunidadesAtivasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(3);
        repo.Setup(r => r.ContarVisitasNoPeriodoAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync(4);
        repo.Setup(r => r.ListarDatasCadastroBeneficiariosAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DateTime> { agora, agora, agora.AddMonths(-1) });
        repo.Setup(r => r.ListarDatasCadastroDoadoresAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DateOnly> { DateOnly.FromDateTime(agora) });
        repo.Setup(r => r.ListarBeneficiariosPorComunidadeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ComunidadeResumoDto> { new("Centro", 7), new("Norte", 5) });

        return repo;
    }

    [Fact]
    public async Task ObterAsync_ComDoadores_DeveRetornarSeisMesesETotais()
    {
        var service = new DashboardService(CriarRepo().Object);

        var result = await service.ObterAsync(incluirDoadores: true);

        result.TotalBeneficiarios.Should().Be(12);
        result.TotalDoadores.Should().Be(5);
        result.VisitasMes.Should().Be(4);
        result.TotalComunidades.Should().Be(3);
        result.EvolucaoMensal.Should().HaveCount(6);
        result.EvolucaoMensal.Last().Beneficiarios.Should().Be(2);
        result.EvolucaoMensal.Last().Doadores.Should().Be(1);
        result.EvolucaoMensal[^2].Beneficiarios.Should().Be(1);
        result.Comunidades.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterAsync_SemDoadores_NaoDeveConsultarNemExporDoadores()
    {
        var repo = CriarRepo();
        var service = new DashboardService(repo.Object);

        var result = await service.ObterAsync(incluirDoadores: false);

        result.TotalDoadores.Should().BeNull();
        result.EvolucaoMensal.Should().OnlyContain(m => m.Doadores == null);
        repo.Verify(r => r.ContarDoadoresAtivosAsync(It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.ListarDatasCadastroDoadoresAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
