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

public class GetOrcamentosMensaisByUsuarioQueryHandlerTests
{
    private readonly Mock<IOrcamentoMensalRepository> _orcamentoRepositoryMock;
    private readonly Mock<IOrcamentoCalculoService> _calculoServiceMock;
    private readonly GetOrcamentosMensaisByUsuarioQueryHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public GetOrcamentosMensaisByUsuarioQueryHandlerTests()
    {
        _orcamentoRepositoryMock = new Mock<IOrcamentoMensalRepository>();
        _calculoServiceMock = new Mock<IOrcamentoCalculoService>();

        _handler = new GetOrcamentosMensaisByUsuarioQueryHandler(
            _orcamentoRepositoryMock.Object,
            _calculoServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_DeveRetornarTodosOrcamentos_DoUsuario()
    {
        // Arrange
        var orcamentos = new List<OrcamentoMensal>
        {
            new OrcamentoMensal(_usuarioId, "2025-10", new Money(5000)),
            new OrcamentoMensal(_usuarioId, "2025-11", new Money(6000)),
            new OrcamentoMensal(_usuarioId, "2025-12", new Money(7000))
        };

        var query = new GetOrcamentosMensaisByUsuarioQuery(_usuarioId);

        _orcamentoRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(orcamentos);

        _calculoServiceMock
            .Setup(s => s.CalcularEstatisticasOrcamentoAsync(_usuarioId, It.IsAny<string>()))
            .ReturnsAsync(new Services.OrcamentoCalculoResultado
            {
                ValorOrcamento = 5000,
                ValorGasto = 3000,
                ValorRestante = 2000,
                PercentualUtilizado = 60,
                Status = Services.StatusOrcamento.Dentro,
                Categoria = "Normal",
                MensagemStatus = "OK"
            });

        _calculoServiceMock
            .Setup(s => s.CalcularPercentuais(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(new Services.OrcamentoCalculoResultado
            {
                ValorOrcamento = 5000,
                ValorGasto = 3000,
                ValorRestante = 2000,
                PercentualUtilizado = 60,
                Status = Services.StatusOrcamento.Dentro,
                Categoria = "Normal",
                MensagemStatus = "OK"
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.First().AnoMes.Should().Be("2025-12"); // Ordenado por AnoMes descendente
        _calculoServiceMock.Verify(
            s => s.CalcularEstatisticasOrcamentoAsync(_usuarioId, It.IsAny<string>()),
            Times.Exactly(3)
        );
    }

    [Fact]
    public async Task Handle_DeveRetornarListaVazia_QuandoUsuarioNaoTemOrcamentos()
    {
        // Arrange
        var query = new GetOrcamentosMensaisByUsuarioQuery(_usuarioId);

        _orcamentoRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<OrcamentoMensal>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _calculoServiceMock.Verify(
            s => s.CalcularEstatisticasOrcamentoAsync(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_DeveOrdenarPorAnoMes_Descendente()
    {
        // Arrange
        var orcamentos = new List<OrcamentoMensal>
        {
            new OrcamentoMensal(_usuarioId, "2025-01", new Money(5000)),
            new OrcamentoMensal(_usuarioId, "2025-12", new Money(6000)),
            new OrcamentoMensal(_usuarioId, "2025-06", new Money(7000))
        };

        var query = new GetOrcamentosMensaisByUsuarioQuery(_usuarioId);

        _orcamentoRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(orcamentos);

        _calculoServiceMock
            .Setup(s => s.CalcularEstatisticasOrcamentoAsync(_usuarioId, It.IsAny<string>()))
            .ReturnsAsync(new Services.OrcamentoCalculoResultado
            {
                ValorOrcamento = 5000,
                ValorGasto = 1000,
                ValorRestante = 4000,
                PercentualUtilizado = 20,
                Status = Services.StatusOrcamento.Dentro,
                Categoria = "Normal",
                MensagemStatus = "OK"
            });

        _calculoServiceMock
            .Setup(s => s.CalcularPercentuais(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(new Services.OrcamentoCalculoResultado
            {
                ValorOrcamento = 5000,
                ValorGasto = 1000,
                ValorRestante = 4000,
                PercentualUtilizado = 20,
                Status = Services.StatusOrcamento.Dentro,
                Categoria = "Normal",
                MensagemStatus = "OK"
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList[0].AnoMes.Should().Be("2025-12");
        resultList[1].AnoMes.Should().Be("2025-06");
        resultList[2].AnoMes.Should().Be("2025-01");
    }
}
