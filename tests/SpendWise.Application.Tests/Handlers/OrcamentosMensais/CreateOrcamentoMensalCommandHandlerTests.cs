using FluentAssertions;
using Moq;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.OrcamentosMensais;

public class CreateOrcamentoMensalCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrcamentoMensalRepository> _orcamentoRepositoryMock;
    private readonly Guid _usuarioId;

    public CreateOrcamentoMensalCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orcamentoRepositoryMock = new Mock<IOrcamentoMensalRepository>();

        _unitOfWorkMock.Setup(u => u.OrcamentosMensais).Returns(_orcamentoRepositoryMock.Object);

        _usuarioId = Guid.NewGuid();
    }

    [Fact]
    public void DeveCriarOrcamentoComValorPositivo()
    {
        // Arrange & Act
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(5000m));

        // Assert
        orcamento.Should().NotBeNull();
        orcamento.AnoMes.Should().Be("2025-10");
        orcamento.Valor.Valor.Should().Be(5000m);
    }

    [Fact]
    public void DeveManterUsuarioIdCorreto()
    {
        // Arrange & Act
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(5000m));

        // Assert
        orcamento.UsuarioId.Should().Be(_usuarioId);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void DeveAceitarDiferentesValores(decimal valor)
    {
        // Arrange & Act
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(valor));

        // Assert
        orcamento.Valor.Valor.Should().Be(valor);
    }

    [Fact]
    public void DeveAceitarValorZero()
    {
        // Arrange & Act
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(0m));

        // Assert
        orcamento.Valor.Valor.Should().Be(0m);
    }

    [Theory]
    [InlineData("2025-01")]
    [InlineData("2024-12")]
    [InlineData("2026-06")]
    public void DeveAceitarDiferentesPeriodos(string anoMes)
    {
        // Arrange & Act
        var orcamento = new OrcamentoMensal(_usuarioId, anoMes, new Money(5000m));

        // Assert
        orcamento.AnoMes.Should().Be(anoMes);
    }
}
