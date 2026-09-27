using FluentAssertions;
using Moq;
using SolidarSYS.Application.Comunidades.DTOs;
using SolidarSYS.Application.Comunidades.Interfaces;
using SolidarSYS.Application.Comunidades.Services;
using SolidarSYS.Domain.Entities;
using Xunit;

namespace SolidarSYS.Tests;

public class ComunidadeServiceTests
{
    [Fact]
    public async Task ListarAsync_ShouldReturnMappedPagedResult()
    {
        var repo = new Mock<IComunidadeRepository>();
        var comunidade = new Comunidade
        {
            IdComunidade = 1,
            Nome = "Comunidade A",
            Regiao = "Norte",
            Descricao = "Descricao A",
            Ativa = true,
            DataCriacao = DateTime.UtcNow,
            TotalBeneficiarios = 10
        };

        repo.Setup(r => r.ListarAsync(1, 10, "A", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Comunidade> { comunidade }, 1));

        var service = new ComunidadeService(repo.Object);

        var result = await service.ListarAsync(1, 10, "A", true);

        result.Total.Should().Be(1);
        result.Pagina.Should().Be(1);
        result.TamanhoPagina.Should().Be(10);
        result.Items.Should().HaveCount(1);
        result.Items[0].IdComunidade.Should().Be(1);
        result.Items[0].Nome.Should().Be("Comunidade A");
        result.Items[0].Regiao.Should().Be("Norte");
        result.Items[0].TotalBeneficiarios.Should().Be(10);
    }

    [Fact]
    public async Task ObterPorIdAsync_ShouldThrow_WhenComunidadeDoesNotExist()
    {
        var repo = new Mock<IComunidadeRepository>();
        repo.Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comunidade?)null);

        var service = new ComunidadeService(repo.Object);

        var act = () => service.ObterPorIdAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Comunidade não encontrada.");
    }

    [Fact]
    public async Task CriarAsync_ShouldRejectDuplicateName()
    {
        var repo = new Mock<IComunidadeRepository>();
        repo.Setup(r => r.BuscarPorNomeAsync("Comunidade A", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Comunidade { IdComunidade = 1, Nome = "Comunidade A" });

        var service = new ComunidadeService(repo.Object);
        var dto = new CriarComunidadeDto("Comunidade A", "Norte", "Descricao");

        var act = () => service.CriarAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe uma comunidade cadastrada com este nome");

        repo.Verify(r => r.CriarAsync(It.IsAny<Comunidade>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ShouldCreateAndPersistCommunity()
    {
        var repo = new Mock<IComunidadeRepository>();
        var dto = new CriarComunidadeDto("Comunidade A", "Norte", "Descricao");

        repo.Setup(r => r.BuscarPorNomeAsync(dto.Nome, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comunidade?)null);

        repo.Setup(r => r.CriarAsync(It.IsAny<Comunidade>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comunidade c, CancellationToken _) => c);

        repo.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ComunidadeService(repo.Object);

        var result = await service.CriarAsync(dto);

        result.Nome.Should().Be("Comunidade A");
        result.Regiao.Should().Be("Norte");
        result.TotalBeneficiarios.Should().Be(0);

        repo.Verify(r => r.CriarAsync(It.IsAny<Comunidade>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ShouldUpdateAndPersistCommunity()
    {
        var repo = new Mock<IComunidadeRepository>();
        var entidade = new Comunidade
        {
            IdComunidade = 1,
            Nome = "Antigo",
            Regiao = "Sul",
            Descricao = "Antiga",
            Ativa = true,
            DataCriacao = DateTime.UtcNow,
            TotalBeneficiarios = 0
        };

        repo.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entidade);

        repo.Setup(r => r.AtualizarAsync(entidade, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        repo.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ComunidadeService(repo.Object);
        var dto = new AtualizarComunidadeDto("Novo", "Centro", "Nova descricao");

        var result = await service.AtualizarAsync(1, dto);

        result.Nome.Should().Be("Novo");
        result.Regiao.Should().Be("Centro");
        entidade.Nome.Should().Be("Novo");
        entidade.Regiao.Should().Be("Centro");
        entidade.Descricao.Should().Be("Nova descricao");

        repo.Verify(r => r.AtualizarAsync(entidade, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlternarAtivoAsync_ShouldToggleAndPersist()
    {
        var repo = new Mock<IComunidadeRepository>();
        var entidade = new Comunidade
        {
            IdComunidade = 1,
            Nome = "Comunidade A",
            Regiao = "Norte",
            Descricao = "Descricao",
            Ativa = true,
            DataCriacao = DateTime.UtcNow,
            TotalBeneficiarios = 0
        };

        repo.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entidade);

        repo.Setup(r => r.AtualizarAsync(entidade, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        repo.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ComunidadeService(repo.Object);

        await service.AlternarAtivoAsync(1);

        entidade.Ativa.Should().BeFalse();
        repo.Verify(r => r.AtualizarAsync(entidade, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
