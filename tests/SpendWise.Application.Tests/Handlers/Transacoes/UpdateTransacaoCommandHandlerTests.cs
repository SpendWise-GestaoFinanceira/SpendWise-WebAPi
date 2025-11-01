using AutoMapper;
using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.Transacoes;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Exceptions;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Transacoes;

public class UpdateTransacaoCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<IFechamentoMensalRepository> _fechamentoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateTransacaoCommandHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _categoriaId = Guid.NewGuid();
    private readonly Guid _transacaoId = Guid.NewGuid();

    public UpdateTransacaoCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _fechamentoRepositoryMock = new Mock<IFechamentoMensalRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FechamentosMensais).Returns(_fechamentoRepositoryMock.Object);

        _handler = new UpdateTransacaoCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAtualizarTransacao_ComSucesso()
    {
        // Arrange
        var transacao = new Transacao(
            "Compra Original",
            new Money(100),
            new DateTime(2025, 10, 15),
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId
        );

        var command = new UpdateTransacaoCommand(
            _transacaoId,
            "Compra Atualizada",
            new Money(150),
            new DateTime(2025, 10, 20),
            TipoTransacao.Despesa,
            _categoriaId,
            "Observação atualizada"
        );

        var transacaoDto = new TransacaoDto { Id = _transacaoId, Descricao = "Compra Atualizada" };

        _transacaoRepositoryMock
            .Setup(r => r.GetByIdAsync(_transacaoId))
            .ReturnsAsync(transacao);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, It.IsAny<string>()))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(m => m.Map<TransacaoDto>(transacao))
            .Returns(transacaoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Descricao.Should().Be("Compra Atualizada");
        _transacaoRepositoryMock.Verify(r => r.UpdateAsync(transacao), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarNull_QuandoTransacaoNaoExiste()
    {
        // Arrange
        var command = new UpdateTransacaoCommand(
            _transacaoId,
            "Compra",
            new Money(100),
            DateTime.Now,
            TipoTransacao.Despesa,
            _categoriaId,
            null
        );

        _transacaoRepositoryMock
            .Setup(r => r.GetByIdAsync(_transacaoId))
            .ReturnsAsync((Transacao?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _transacaoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Transacao>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoMesOriginalFechado()
    {
        // Arrange
        var transacao = new Transacao(
            "Compra",
            new Money(100),
            new DateTime(2025, 10, 15),
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId
        );

        var command = new UpdateTransacaoCommand(
            _transacaoId,
            "Compra Atualizada",
            new Money(150),
            new DateTime(2025, 10, 20),
            TipoTransacao.Despesa,
            _categoriaId,
            null
        );

        _transacaoRepositoryMock
            .Setup(r => r.GetByIdAsync(_transacaoId))
            .ReturnsAsync(transacao);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<MesFechadoException>();
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoMesNovoFechado()
    {
        // Arrange
        var transacao = new Transacao(
            "Compra",
            new Money(100),
            new DateTime(2025, 10, 15),
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId
        );

        var command = new UpdateTransacaoCommand(
            _transacaoId,
            "Compra Atualizada",
            new Money(150),
            new DateTime(2025, 11, 20), // Mudou o mês
            TipoTransacao.Despesa,
            _categoriaId,
            null
        );

        _transacaoRepositoryMock
            .Setup(r => r.GetByIdAsync(_transacaoId))
            .ReturnsAsync(transacao);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(false);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-11"))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<MesFechadoException>();
    }
}
