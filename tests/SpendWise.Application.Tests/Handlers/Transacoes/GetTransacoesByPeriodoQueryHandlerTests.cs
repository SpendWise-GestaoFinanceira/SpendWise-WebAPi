using AutoMapper;
using FluentAssertions;
using Moq;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.Transacoes;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Transacoes;

public class GetTransacoesByPeriodoQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetTransacoesByPeriodoQueryHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _categoriaId = Guid.NewGuid();

    public GetTransacoesByPeriodoQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);

        _handler = new GetTransacoesByPeriodoQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarTransacoes_DentroDoPeriodo()
    {
        // Arrange
        var inicio = new DateTime(2025, 10, 1);
        var fim = new DateTime(2025, 10, 31);
        var periodo = new Periodo(inicio, fim);

        var transacoes = new List<Transacao>
        {
            new Transacao("Compra 1", new Money(100), new DateTime(2025, 10, 5), TipoTransacao.Despesa, _usuarioId, _categoriaId),
            new Transacao("Compra 2", new Money(200), new DateTime(2025, 10, 15), TipoTransacao.Despesa, _usuarioId, _categoriaId),
            new Transacao("Receita 1", new Money(500), new DateTime(2025, 10, 20), TipoTransacao.Receita, _usuarioId, _categoriaId)
        };

        var transacoesDto = new List<TransacaoDto>
        {
            new TransacaoDto { Id = Guid.NewGuid(), Descricao = "Compra 1" },
            new TransacaoDto { Id = Guid.NewGuid(), Descricao = "Compra 2" },
            new TransacaoDto { Id = Guid.NewGuid(), Descricao = "Receita 1" }
        };

        var query = new GetTransacoesByPeriodoQuery(inicio, fim, _usuarioId);

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(transacoes);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<TransacaoDto>>(transacoes))
            .Returns(transacoesDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        _transacaoRepositoryMock.Verify(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarListaVazia_QuandoNaoHaTransacoes()
    {
        // Arrange
        var inicio = new DateTime(2025, 10, 1);
        var fim = new DateTime(2025, 10, 31);
        var query = new GetTransacoesByPeriodoQuery(inicio, fim, _usuarioId);

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(new List<Transacao>());

        _mapperMock
            .Setup(m => m.Map<IEnumerable<TransacaoDto>>(It.IsAny<List<Transacao>>()))
            .Returns(new List<TransacaoDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DeveFiltrarPorTipo_QuandoEspecificado()
    {
        // Arrange
        var inicio = new DateTime(2025, 10, 1);
        var fim = new DateTime(2025, 10, 31);

        var transacoes = new List<Transacao>
        {
            new Transacao("Despesa 1", new Money(100), new DateTime(2025, 10, 5), TipoTransacao.Despesa, _usuarioId, _categoriaId),
            new Transacao("Despesa 2", new Money(200), new DateTime(2025, 10, 15), TipoTransacao.Despesa, _usuarioId, _categoriaId)
        };

        var transacoesDto = new List<TransacaoDto>
        {
            new TransacaoDto { Id = Guid.NewGuid(), Descricao = "Despesa 1" },
            new TransacaoDto { Id = Guid.NewGuid(), Descricao = "Despesa 2" }
        };

        var query = new GetTransacoesByPeriodoQuery(inicio, fim, _usuarioId);

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(transacoes);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<TransacaoDto>>(transacoes))
            .Returns(transacoesDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(t => t.Descricao.Contains("Despesa")).Should().BeTrue();
    }
}
