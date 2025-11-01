using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Application.Handlers.Transacoes;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Exceptions;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Transacoes;

public class DeleteTransacaoCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<IFechamentoMensalRepository> _fechamentoRepositoryMock;
    private readonly DeleteTransacaoCommandHandler _handler;
    private readonly Guid _transacaoId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _categoriaId = Guid.NewGuid();

    public DeleteTransacaoCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _fechamentoRepositoryMock = new Mock<IFechamentoMensalRepository>();

        _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FechamentosMensais).Returns(_fechamentoRepositoryMock.Object);

        _handler = new DeleteTransacaoCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_DeveDeletarTransacao_ComSucesso()
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

        var command = new DeleteTransacaoCommand(_transacaoId);

        _transacaoRepositoryMock
            .Setup(r => r.GetByIdAsync(_transacaoId))
            .ReturnsAsync(transacao);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _transacaoRepositoryMock.Verify(r => r.DeleteAsync(_transacaoId), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalse_QuandoTransacaoNaoExiste()
    {
        // Arrange
        var command = new DeleteTransacaoCommand(_transacaoId);

        _transacaoRepositoryMock
            .Setup(r => r.GetByIdAsync(_transacaoId))
            .ReturnsAsync((Transacao?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _transacaoRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoMesEstaFechado()
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

        var command = new DeleteTransacaoCommand(_transacaoId);

        _transacaoRepositoryMock
            .Setup(r => r.GetByIdAsync(_transacaoId))
            .ReturnsAsync(transacao);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<MesFechadoException>();
        
        _transacaoRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
