using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using SpendWise.Application.Commands.OrcamentosMensais;
using SpendWise.Application.Handlers.OrcamentosMensais;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.OrcamentosMensais;

public class DeleteOrcamentoMensalHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrcamentoMensalRepository> _orcamentoRepositoryMock;
    private readonly Mock<IFechamentoMensalRepository> _fechamentoRepositoryMock;
    private readonly Mock<IValidator<DeleteOrcamentoMensalCommand>> _validatorMock;
    private readonly DeleteOrcamentoMensalHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _orcamentoId = Guid.NewGuid();

    public DeleteOrcamentoMensalHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orcamentoRepositoryMock = new Mock<IOrcamentoMensalRepository>();
        _fechamentoRepositoryMock = new Mock<IFechamentoMensalRepository>();
        _validatorMock = new Mock<IValidator<DeleteOrcamentoMensalCommand>>();

        _unitOfWorkMock
            .Setup(u => u.OrcamentosMensais)
            .Returns(_orcamentoRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(u => u.FechamentosMensais)
            .Returns(_fechamentoRepositoryMock.Object);

        // Setup validator para retornar sucesso por padrão
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteOrcamentoMensalCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = new DeleteOrcamentoMensalHandler(_unitOfWorkMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task Handle_DeveDeletarOrcamento_QuandoExiste()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(5000));

        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync(orcamento);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(false);

        var command = new DeleteOrcamentoMensalCommand(_orcamentoId, _usuarioId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _orcamentoRepositoryMock.Verify(r => r.Delete(orcamento), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalse_QuandoNaoExiste()
    {
        // Arrange
        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync((OrcamentoMensal?)null);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(false);

        var command = new DeleteOrcamentoMensalCommand(_orcamentoId, _usuarioId);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();

        _orcamentoRepositoryMock.Verify(r => r.Delete(It.IsAny<OrcamentoMensal>()), Times.Never);
    }

}
