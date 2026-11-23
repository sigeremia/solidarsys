using FluentAssertions;
using Moq;
using SolidarSYS.Application.Relatorios.DTOs;
using SolidarSYS.Application.Relatorios.Interfaces;
using SolidarSYS.Application.Relatorios.Services;
using Xunit;

namespace SolidarSYS.Tests;

public class RelatorioServiceTests
{
    private static Mock<IRelatorioRepository> CriarRepo(
        IList<DateTime>? cadastros = null,
        IList<DoacaoPeriodoDto>? doacoes = null)
    {
        var repo = new Mock<IRelatorioRepository>();
        var agora = DateTime.UtcNow;

        repo.Setup(r => r.ContarBeneficiariosAtivosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(12);
        repo.Setup(r => r.ContarComunidadesAtendidasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);
        repo.Setup(r => r.ListarBeneficiariosPorComunidadeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RelatorioComunidadeDto> { new("Centro", 7), new("Norte", 5) });
        repo.Setup(r => r.ListarDatasCadastroBeneficiariosAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cadastros ?? new List<DateTime> { agora, agora, agora.AddMonths(-1) });
        repo.Setup(r => r.ListarDoacoesNoPeriodoAsync(
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(doacoes ?? new List<DoacaoPeriodoDto>
            {
                new(DateOnly.FromDateTime(agora), 100m),
                new(DateOnly.FromDateTime(agora), 50.5m),
                new(DateOnly.FromDateTime(agora.AddMonths(-1)), 200m),
            });

        return repo;
    }

    [Theory]
    [InlineData(null, "semestral", 6)]
    [InlineData("", "semestral", 6)]
    [InlineData("trimestral", "trimestral", 3)]
    [InlineData("SEMESTRAL", "semestral", 6)]
    [InlineData("anual", "anual", 12)]
    public async Task ObterAsync_DeveGerarUmPontoPorMes(string? periodo, string esperado, int pontos)
    {
        var service = new RelatorioService(CriarRepo().Object);

        var result = await service.ObterAsync(periodo);

        result.Periodo.Should().Be(esperado);
        result.Granularidade.Should().Be("MES");
        result.Serie.Should().HaveCount(pontos);
    }

    [Fact]
    public async Task ObterAsync_DeveConsolidarTotaisECardsDoPeriodo()
    {
        var service = new RelatorioService(CriarRepo().Object);

        var result = await service.ObterAsync("semestral");

        result.TotalBeneficiarios.Should().Be(12);
        result.ComunidadesAtendidas.Should().Be(2);
        result.TotalDoacoes.Should().Be(350.5m);
        result.SemDados.Should().BeFalse();
        result.Comunidades.Should().HaveCount(2);
        result.Serie.Last().Beneficiarios.Should().Be(2);
        result.Serie.Last().Doacoes.Should().Be(150.5m);
        result.Serie[^2].Beneficiarios.Should().Be(1);
        result.Serie[^2].Doacoes.Should().Be(200m);
    }

    [Fact]
    public async Task ObterAsync_Mensal_DeveGerarUmPontoPorDiaDoMesAtualAteHoje()
    {
        var service = new RelatorioService(CriarRepo().Object);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await service.ObterAsync("mensal");

        result.Granularidade.Should().Be("DIA");
        result.Serie.Should().HaveCount(hoje.Day);
        result.Serie.First().Data.Should().Be(new DateOnly(hoje.Year, hoje.Month, 1));
        result.Serie.Last().Data.Should().Be(hoje);
        result.Serie.Last().Beneficiarios.Should().Be(2);
    }

    [Fact]
    public async Task ObterAsync_SemCadastrosNemDoacoes_DeveMarcarSemDadosEManterSerieZerada()
    {
        var repo = CriarRepo(new List<DateTime>(), new List<DoacaoPeriodoDto>());
        var service = new RelatorioService(repo.Object);

        var result = await service.ObterAsync("trimestral");

        result.SemDados.Should().BeTrue();
        result.TotalDoacoes.Should().Be(0m);
        result.Serie.Should().HaveCount(3);
        result.Serie.Should().OnlyContain(p => p.Beneficiarios == 0 && p.Doacoes == 0m);
    }

    [Fact]
    public async Task ObterAsync_PeriodoInvalido_DeveLancarArgumentException()
    {
        var service = new RelatorioService(CriarRepo().Object);

        var acao = () => service.ObterAsync("decenal");

        await acao.Should().ThrowAsync<ArgumentException>();
    }
}
