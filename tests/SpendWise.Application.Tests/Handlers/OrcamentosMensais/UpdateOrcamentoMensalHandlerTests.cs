using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using SpendWise.Application.Commands.OrcamentosMensais;
using SpendWise.Application.Handlers.OrcamentosMensais;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Exceptions;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.OrcamentosMensais;

public class UpdateOrcamentoMensalHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrcamentoMensalRepository> _orcamentoRepositoryMock;
    private readonly Mock<IFechamentoMensalRepository> _fechamentoRepositoryMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<IValidator<UpdateOrcamentoMensalCommand>> _validatorMock;
    private readonly UpdateOrcamentoMensalHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _orcamentoId = Guid.NewGuid();

    public UpdateOrcamentoMensalHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orcamentoRepositoryMock = new Mock<IOrcamentoMensalRepository>();
        _fechamentoRepositoryMock = new Mock<IFechamentoMensalRepository>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _validatorMock = new Mock<IValidator<UpdateOrcamentoMensalCommand>>();

        _unitOfWorkMock.Setup(u => u.OrcamentosMensais).Returns(_orcamentoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FechamentosMensais).Returns(_fechamentoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateOrcamentoMensalCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = new UpdateOrcamentoMensalHandler(_unitOfWorkMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAtualizarOrcamento_ComSucesso()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(3000));
        var command = new UpdateOrcamentoMensalCommand(
            _orcamentoId,
            new Money(5000),
            _usuarioId
        );

        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync(orcamento);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(false);

        _transacaoRepositoryMock
            .Setup(r => r.GetTotalByTipoAsync(_usuarioId, TipoTransacao.Despesa, It.IsAny<Periodo>()))
            .ReturnsAsync(2000);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Valor.Valor.Should().Be(5000);
        result.ValorGasto.Should().Be(2000);
        result.ValorRestante.Should().Be(3000);
        _orcamentoRepositoryMock.Verify(r => r.Update(orcamento), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoOrcamentoNaoExiste()
    {
        // Arrange
        var command = new UpdateOrcamentoMensalCommand(
            _orcamentoId,
            new Money(5000),
            _usuarioId
        );

        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync((OrcamentoMensal?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"*{_orcamentoId}*");
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoMesEstaFechado()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-10", new Money(3000));
        var command = new UpdateOrcamentoMensalCommand(
            _orcamentoId,
            new Money(5000),
            _usuarioId
        );

        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync(orcamento);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<MesFechadoException>();
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoUsuarioNaoTemPermissao()
    {
        // Arrange
        var outroUsuarioId = Guid.NewGuid();
        var orcamento = new OrcamentoMensal(outroUsuarioId, "2025-10", new Money(3000));
        var command = new UpdateOrcamentoMensalCommand(
            _orcamentoId,
            new Money(5000),
            _usuarioId
        );

        _orcamentoRepositoryMock
            .Setup(r => r.GetByIdAsync(_orcamentoId))
            .ReturnsAsync(orcamento);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(outroUsuarioId, "2025-10"))
            .ReturnsAsync(false);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
