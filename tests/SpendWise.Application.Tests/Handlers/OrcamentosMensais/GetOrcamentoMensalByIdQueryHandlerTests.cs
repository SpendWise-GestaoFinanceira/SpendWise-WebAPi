using FluentAssertions;
using Moq;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.OrcamentosMensais;
using SpendWise.Application.Queries.OrcamentosMensais;
using SpendWise.Application.Services;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.OrcamentosMensais;

public class GetOrcamentoMensalByIdQueryHandlerTests
{
    private readonly Mock<IOrcamentoMensalRepository> _orcamentoRepositoryMock;
    private readonly Mock<IOrcamentoCalculoService> _calculoServiceMock;
    private readonly GetOrcamentoMensalByIdQueryHandler _handler;
    private readonly Guid _orcamentoId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();

    public GetOrcamentoMensalByIdQueryHandlerTests()
    {
        _orcamentoRepositoryMock = new Mock<IOrcamentoMensalRepository>();
        _calculoServiceMock = new Mock<IOrcamentoCalculoService>();

        _handler = new GetOrcamentoMensalByIdQueryHandler(
            _orcamentoRepositoryMock.Object,
            _calculoServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_DeveRetornarOrcamento_QuandoExiste()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(5000));
        var query = new GetOrcamentoMensalByIdQuery(_orcamentoId, _usuarioId);

        var calculo = new Services.OrcamentoCalculoResultado
        {
            ValorOrcamento = 5000,
            ValorGasto = 3000,
            ValorRestante = 2000,
            PercentualUtilizado = 60,
            Status = Services.StatusOrcamento.Dentro,
            Categoria = "Normal",
            MensagemStatus = "Dentro do orçamento"
        };

        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync(orcamento);

        _calculoServiceMock
            .Setup(s => s.CalcularEstatisticasOrcamentoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(calculo);

        _calculoServiceMock
            .Setup(s => s.CalcularPercentuais(5000, 3000))
            .Returns(calculo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orcamento.Id);
        result.UsuarioId.Should().Be(_usuarioId);
        result.AnoMes.Should().Be("2025-10");
        result.Valor.Valor.Should().Be(5000);
        result.ValorGasto.Should().Be(3000);
        result.ValorRestante.Should().Be(2000);
        result.PercentualUtilizado.Should().Be(60);
    }

    [Fact]
    public async Task Handle_DeveRetornarNull_QuandoNaoExiste()
    {
        // Arrange
        var query = new GetOrcamentoMensalByIdQuery(_orcamentoId, _usuarioId);

        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync((OrcamentoMensal?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _calculoServiceMock.Verify(
            s => s.CalcularEstatisticasOrcamentoAsync(It.IsAny<Guid>(), It.IsAny<string>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_DeveCalcularEstatisticas_Corretamente()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(10000));
        var query = new GetOrcamentoMensalByIdQuery(_orcamentoId, _usuarioId);

        var calculo = new Services.OrcamentoCalculoResultado
        {
            ValorOrcamento = 10000,
            ValorGasto = 9500,
            ValorRestante = 500,
            PercentualUtilizado = 95,
            Status = Services.StatusOrcamento.Alerta,
            Categoria = "Alerta",
            MensagemStatus = "Atenção ao limite"
        };

        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync(orcamento);

        _calculoServiceMock
            .Setup(s => s.CalcularEstatisticasOrcamentoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(calculo);

        _calculoServiceMock
            .Setup(s => s.CalcularPercentuais(10000, 9500))
            .Returns(calculo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PercentualUtilizado.Should().Be(95);
        result.Status.Should().Be(DTOs.StatusOrcamento.Alerta);
        _calculoServiceMock.Verify(s => s.CalcularEstatisticasOrcamentoAsync(_usuarioId, "2025-10"), Times.Once);
        _calculoServiceMock.Verify(s => s.CalcularPercentuais(10000, 9500), Times.Once);
    }
}
