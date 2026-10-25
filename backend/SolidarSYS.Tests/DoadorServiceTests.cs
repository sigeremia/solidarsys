using FluentAssertions;
using Moq;
using SolidarSYS.Application.Doadores.DTOs;
using SolidarSYS.Application.Doadores.Interfaces;
using SolidarSYS.Application.Doadores.Services;
using SolidarSYS.Application.Doadores.Validators;
using SolidarSYS.Domain.Entities;
using SolidarSYS.Domain.Enums;
using Xunit;

namespace SolidarSYS.Tests;

public class DoadorServiceTests
{
    [Fact]
    public async Task CriarAsync_NormalizaDocumentoETelefone()
    {
        var repo = new Mock<IDoadorRepository>();
        repo.Setup(r => r.ExisteDocumentoAsync("52998224725", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repo.Setup(r => r.AdicionarAsync(It.IsAny<Doador>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new DoadorService(repo.Object);
        var dto = new CriarDoadorDto(
            TipoDoador.PESSOA_FISICA,
            "  Maria Silva  ",
            "529.982.247-25",
            null,
            "(11) 99999-9999",
            null,
            null);

        var result = await service.CriarAsync(dto);

        result.Nome.Should().Be("Maria Silva");
        result.Documento.Should().Be("52998224725");
        result.Telefone.Should().Be("11999999999");
        result.Ativo.Should().BeTrue();
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_RejeitaDocumentoDuplicadoSemSalvar()
    {
        var repo = new Mock<IDoadorRepository>();
        repo.Setup(r => r.ExisteDocumentoAsync("52998224725", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new DoadorService(repo.Object);
        var dto = new CriarDoadorDto(
            TipoDoador.PESSOA_FISICA, "Maria", "529.982.247-25", null, null, null, null);

        var act = () => service.CriarAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um doador cadastrado com este documento.");
        repo.Verify(r => r.AdicionarAsync(It.IsAny<Doador>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListarAsync_MapeiaDataDaDoacaoMaisRecente()
    {
        var doador = new Doador
        {
            IdDoador = 8,
            Tipo = TipoDoador.PESSOA_JURIDICA,
            Nome = "Mercado do Bairro",
            Documento = "11222333000181",
            Doacoes =
            [
                new Doacao { Data = new DateOnly(2025, 12, 1) },
                new Doacao { Data = new DateOnly(2026, 2, 25) },
            ],
        };
        var repo = new Mock<IDoadorRepository>();
        repo.Setup(r => r.ListarAsync(1, 10, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IList<Doador>)new List<Doador> { doador }, 1));
        var service = new DoadorService(repo.Object);

        var result = await service.ListarAsync(1, 10, null, true);

        result.Items.Should().ContainSingle();
        result.Items[0].UltimaDoacao.Should().Be(new DateOnly(2026, 2, 25));
    }

    [Theory]
    [InlineData(TipoDoador.PESSOA_FISICA, "529.982.247-25", true)]
    [InlineData(TipoDoador.PESSOA_FISICA, "111.111.111-11", false)]
    [InlineData(TipoDoador.PESSOA_JURIDICA, "11.222.333/0001-81", true)]
    [InlineData(TipoDoador.PESSOA_JURIDICA, "11.222.333/0001-80", false)]
    [InlineData(TipoDoador.PESSOA_FISICA, "PASSAPORTE-A1", true)]
    [InlineData(TipoDoador.PESSOA_JURIDICA, "REGISTRO-EXT-42", true)]
    public void DoadorDocumento_EhValidoConformeTipo(
        TipoDoador tipo, string documento, bool esperado)
    {
        DoadorDocumento.EhValido(tipo, documento).Should().Be(esperado);
    }
}