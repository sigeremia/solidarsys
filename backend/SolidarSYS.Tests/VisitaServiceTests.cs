using FluentAssertions;
using Moq;
using SolidarSYS.Application.Visitas.DTOs;
using SolidarSYS.Application.Visitas.Interfaces;
using SolidarSYS.Application.Visitas.Services;
using SolidarSYS.Domain.Entities;
using Xunit;

namespace SolidarSYS.Tests;

public class VisitaServiceTests
{
    [Fact]
    public async Task ListarAsync_ShouldReturnMappedPagedResult()
    {
        var repo = new Mock<IVisitaRepository>();
        var visita = new Visita
        {
            IdVisita = 1,
            IdBeneficiario = 7,
            Beneficiario = new Beneficiario
            {
                IdBeneficiario = 7,
                Nome = "Maria Silva",
                Ativo = true,
            },
            DataVisita = new DateOnly(2024, 09, 30),
            Responsavel = "Paula Gestora",
            Observacoes = "Visita de rotina. Família em boas condições.",
            DataCadastro = DateTime.UtcNow,
        };

        repo.Setup(r => r.ListarAsync(1, 10, "maria", It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Visita> { visita }, 1));

        var service = new VisitaService(repo.Object);

        var result = await service.ListarAsync(1, 10, "maria");

        result.Total.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].IdVisita.Should().Be(1);
        result.Items[0].Beneficiario.Should().Be("Maria Silva");
        result.Items[0].Responsavel.Should().Be("Paula Gestora");
    }

    [Fact]
    public async Task CriarAsync_ShouldRejectDuplicateVisitForSameBeneficiaryAndDate()
    {
        var repo = new Mock<IVisitaRepository>();
        repo.Setup(r => r.ObterBeneficiarioAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Beneficiario
            {
                IdBeneficiario = 5,
                Nome = "Maria Silva",
                Ativo = true,
            });

        repo.Setup(r => r.ExisteVisitaNoDiaAsync(5, new DateOnly(2024, 10, 04), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new VisitaService(repo.Object);
        var dto = new CriarVisitaDto(5, new DateOnly(2024, 10, 04), "Marcos Operador", "Entrega de cestas básicas.");

        var act = () => service.CriarAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe uma visita cadastrada para este beneficiário na data informada.");
    }
}
