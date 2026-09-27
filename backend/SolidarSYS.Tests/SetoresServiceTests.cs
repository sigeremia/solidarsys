using FluentAssertions;
using Moq;
using SolidarSYS.Application.Setores.DTOs;
using SolidarSYS.Application.Setores.Interfaces;
using SolidarSYS.Application.Setores.Services;
using SolidarSYS.Domain.Entities;
using Xunit;

namespace SolidarSYS.Tests;

public class SetoresServiceTests
{
    [Fact]
    public async Task ListarAsync_ShouldReturnMappedPagedResult()
    {
        var repo = new Mock<ISetoresRepository>();
        var setores = new Setores
        {
            IdSetores = 1,
            Nome = "Setores A",
            Descricao = "Descricao A",
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        repo.Setup(r => r.ListarAsync(1, 10, "A", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Setores> { setores }, 1));

        var service = new SetoresService(repo.Object);

        var result = await service.ListarAsync(1, 10, "A", true);

        result.Total.Should().Be(1);
        result.Pagina.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].IdSetores.Should().Be(1);
        result.Items[0].Nome.Should().Be("Setores A");
    }

    [Fact]
    public async Task ObterPorIdAsync_ShouldThrow_WhenSetoresDoesNotExist()
    {
        var repo = new Mock<ISetoresRepository>();
        repo.Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comunidade?)null);

        var service = new SetoresService(repo.Object);

        var act = () => service.ObterPorIdAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Setor não encontrado.");
    }

    [Fact]
    public async Task CriarAsync_ShouldRejectDuplicateName()
    {
        var repo = new Mock<ISetoresRepository>();
        repo.Setup(r => r.BuscarPorNomeAsync("Setores A", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Setores { IdSetores = 1, Nome = "Setores A" });

        var service = new SetoresService(repo.Object);
        var dto = new CriarSetoresDto("Setores A", "Descricao");

        var act = () => service.CriarAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um setor cadastrada com este nome");

        repo.Verify(r => r.CriarAsync(It.IsAny<Setores>(), It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ShouldCreateAndPersistSetor()
    {
        var repo = new Mock<ISetoresRepository>();
        var dto = new CriarSetoreDto("Setores A", "Descricao");

        repo.Setup(r => r.BuscarPorNomeAsync(dto.Nome, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Setores?)null);

        repo.Setup(r => r.CriarAsync(It.IsAny<Setores>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Setores c, CancellationToken _) => c);

        repo.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new SetoresService(repo.Object);

        var result = await service.CriarAsync(dto);

        result.Nome.Should().Be("Comunidade A");

        repo.Verify(r => r.CriarAsync(It.IsAny<Setores>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ShouldUpdateAndPersistSetores()
    {
        var repo = new Mock<IComunidadeRepository>();
        var entidade = new Setores
        {
            IdSetores = 1,
            Nome = "Antigo",
            Descricao = "Antiga",
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        repo.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entidade);

        repo.Setup(r => r.AtualizarAsync(entidade, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        repo.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new SetoresService(repo.Object);
        var dto = new AtualizarSetoresDto("Novo", "Nova descricao");

        var result = await service.AtualizarAsync(1, dto);

        result.Nome.Should().Be("Novo");
        entidade.Nome.Should().Be("Novo");
        entidade.Descricao.Should().Be("Nova descricao");

        repo.Verify(r => r.AtualizarAsync(entidade, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlternarAtivoAsync_ShouldToggleAndPersist()
    {
        var repo = new Mock<ISetoresRepository>();
        var entidade = new Setores {
            IdSetores = 1,
            Nome = "Setores A",
            Descricao = "Descricao",
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        repo.Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entidade);

        repo.Setup(r => r.AtualizarAsync(entidade, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        repo.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new SetoresService(repo.Object);

        await service.AlternarAtivoAsync(1);

        entidade.Ativo.Should().BeFalse();
        repo.Verify(r => r.AtualizarAsync(entidade, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
