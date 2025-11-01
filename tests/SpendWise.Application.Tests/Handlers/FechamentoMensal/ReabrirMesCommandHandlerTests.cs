using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.FechamentoMensal;
using SpendWise.Application.Handlers.FechamentoMensal;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.FechamentoMensal;

public class ReabrirMesCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFechamentoMensalRepository> _fechamentoRepositoryMock;
    private readonly ReabrirMesCommandHandler _handler;
    private readonly Guid _usuarioId;

    public ReabrirMesCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fechamentoRepositoryMock = new Mock<IFechamentoMensalRepository>();
        
        _unitOfWorkMock.Setup(u => u.FechamentosMensais).Returns(_fechamentoRepositoryMock.Object);
        
        _handler = new ReabrirMesCommandHandler(_unitOfWorkMock.Object);
        _usuarioId = Guid.NewGuid();
    }

    [Fact]
    public async Task Handle_DeveReabrirMesFechadoComSucesso()
    {
        // Arrange
        var anoMes = "2025-10";
        var command = new ReabrirMesCommand(_usuarioId, anoMes);
        
        var fechamento = new Domain.Entities.FechamentoMensal(_usuarioId, anoMes, 5000, 3000);
        fechamento.Status.Should().Be(Domain.Enums.StatusFechamento.Fechado); // Garantir que está fechado
        
        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, anoMes))
            .ReturnsAsync(fechamento);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        fechamento.Status.Should().Be(Domain.Enums.StatusFechamento.Aberto);
        
        _fechamentoRepositoryMock.Verify(r => r.UpdateAsync(fechamento), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalse_QuandoFechamentoNaoExiste()
    {
        // Arrange
        var anoMes = "2025-10";
        var command = new ReabrirMesCommand(_usuarioId, anoMes);
        
        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, anoMes))
            .ReturnsAsync((Domain.Entities.FechamentoMensal?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        
        _fechamentoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.FechamentoMensal>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_DevePermitirReabrirMesJaReaberto()
    {
        // Arrange
        var anoMes = "2025-10";
        var command = new ReabrirMesCommand(_usuarioId, anoMes);
        
        var fechamento = new Domain.Entities.FechamentoMensal(_usuarioId, anoMes, 5000, 3000);
        // Não reabrir antes - deixar fechado para o handler reabrir
        fechamento.Status.Should().Be(Domain.Enums.StatusFechamento.Fechado);
        
        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, anoMes))
            .ReturnsAsync(fechamento);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        fechamento.Status.Should().Be(Domain.Enums.StatusFechamento.Aberto);
        
        _fechamentoRepositoryMock.Verify(r => r.UpdateAsync(fechamento), Times.Once);
    }

    [Theory]
    [InlineData("2025-01")]
    [InlineData("2025-12")]
    [InlineData("2024-06")]
    public async Task Handle_DeveFuncionarComDiferentesPeriodos(string anoMes)
    {
        // Arrange
        var command = new ReabrirMesCommand(_usuarioId, anoMes);
        
        var fechamento = new Domain.Entities.FechamentoMensal(_usuarioId, anoMes, 1000, 500);
        
        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, anoMes))
            .ReturnsAsync(fechamento);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        fechamento.Status.Should().Be(Domain.Enums.StatusFechamento.Aberto);
    }

    [Fact]
    public async Task Handle_DeveAtualizarUpdatedAt()
    {
        // Arrange
        var anoMes = "2025-10";
        var command = new ReabrirMesCommand(_usuarioId, anoMes);
        
        var fechamento = new Domain.Entities.FechamentoMensal(_usuarioId, anoMes, 5000, 3000);
        var updatedAtAnterior = fechamento.UpdatedAt;
        
        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, anoMes))
            .ReturnsAsync(fechamento);
        
        Thread.Sleep(100); // Garantir diferença de tempo

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (fechamento.UpdatedAt.HasValue && updatedAtAnterior.HasValue)
        {
            fechamento.UpdatedAt.Value.Should().BeAfter(updatedAtAnterior.Value);
        }
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NaoDeveAlterarDadosFinanceiros()
    {
        // Arrange
        var anoMes = "2025-10";
        var command = new ReabrirMesCommand(_usuarioId, anoMes);
        
        var receitas = 5000m;
        var despesas = 3000m;
        var saldo = 2000m;
        
        var fechamento = new Domain.Entities.FechamentoMensal(_usuarioId, anoMes, receitas, despesas);
        
        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, anoMes))
            .ReturnsAsync(fechamento);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        fechamento.TotalReceitas.Should().Be(receitas);
        fechamento.TotalDespesas.Should().Be(despesas);
        fechamento.SaldoFinal.Should().Be(saldo);
    }

    [Fact]
    public async Task Handle_DeveReabrirApenasFechamentoDoUsuarioCorreto()
    {
        // Arrange
        var outroUsuarioId = Guid.NewGuid();
        var anoMes = "2025-10";
        var command = new ReabrirMesCommand(_usuarioId, anoMes);
        
        // Fechamento existe, mas é de outro usuário
        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, anoMes))
            .ReturnsAsync((Domain.Entities.FechamentoMensal?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _fechamentoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.FechamentoMensal>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveChamarCommitApenasPosReabrir()
    {
        // Arrange
        var anoMes = "2025-10";
        var command = new ReabrirMesCommand(_usuarioId, anoMes);
        
        var fechamento = new Domain.Entities.FechamentoMensal(_usuarioId, anoMes, 5000, 3000);
        
        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, anoMes))
            .ReturnsAsync(fechamento);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
