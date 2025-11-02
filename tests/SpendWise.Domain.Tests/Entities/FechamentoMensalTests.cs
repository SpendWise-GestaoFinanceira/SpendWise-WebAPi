using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using Xunit;

namespace SpendWise.Domain.Tests.Entities;

public class FechamentoMensalTests
{
    private readonly Guid _usuarioId;

    public FechamentoMensalTests()
    {
        _usuarioId = Guid.NewGuid();
    }

    [Fact]
    public void CriarFechamentoMensal_DeveInicializarCorretamente()
    {
        // Arrange & Act
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", 5000, 3500);

        // Assert
        fechamento.UsuarioId.Should().Be(_usuarioId);
        fechamento.AnoMes.Should().Be("2025-10");
        fechamento.TotalReceitas.Should().Be(5000);
        fechamento.TotalDespesas.Should().Be(3500);
        fechamento.SaldoFinal.Should().Be(1500);
        fechamento.Status.Should().Be(StatusFechamento.Fechado);
        fechamento.DataFechamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void CriarFechamentoMensal_DeveLancarException_QuandoUsuarioIdVazio()
    {
        // Arrange & Act & Assert
        var act = () => new FechamentoMensal(Guid.Empty, "2025-10", 1000, 500);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*UsuarioId*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void CriarFechamentoMensal_DeveLancarException_QuandoAnoMesInvalidoOuVazio(string anoMes)
    {
        // Arrange & Act & Assert
        var act = () => new FechamentoMensal(_usuarioId, anoMes, 1000, 500);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*AnoMes*");
    }

    [Fact]
    public void CriarFechamentoMensal_DeveLancarException_QuandoAnoMesNull()
    {
        // Arrange & Act & Assert
        var act = () => new FechamentoMensal(_usuarioId, null!, 1000, 500);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*AnoMes*");
    }

    [Fact]
    public void CriarFechamentoMensal_DeveAceitarValoresZero()
    {
        // Arrange & Act
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", 0, 0);

        // Assert
        fechamento.TotalReceitas.Should().Be(0);
        fechamento.TotalDespesas.Should().Be(0);
        fechamento.SaldoFinal.Should().Be(0);
    }

    [Fact]
    public void CriarFechamentoMensal_DeveCalcularSaldoNegativo_QuandoDespesasMaiorQueReceitas()
    {
        // Arrange & Act
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", 2000, 3000);

        // Assert
        fechamento.SaldoFinal.Should().Be(-1000);
    }

    [Fact]
    public void Reabrir_DeveAlterarStatusParaFalse()
    {
        // Arrange
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", 5000, 3000);
        fechamento.Status.Should().Be(StatusFechamento.Fechado);

        // Act
        fechamento.Reabrir();

        // Assert
        fechamento.Status.Should().Be(StatusFechamento.Aberto);
    }

    [Fact]
    public void Reabrir_DeveAtualizarUpdatedAt()
    {
        // Arrange
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", 5000, 3000);
        var updatedAtAnterior = fechamento.UpdatedAt;

        Thread.Sleep(100); // Garantir diferença de tempo

        // Act
        fechamento.Reabrir();

        // Assert
        fechamento.UpdatedAt.Should().NotBeNull();
        if (updatedAtAnterior.HasValue)
        {
            fechamento.UpdatedAt!.Value.Should().BeAfter(updatedAtAnterior.Value);
        }
    }

    [Fact]
    public void Reabrir_DeveLancarException_QuandoJaEstaAberto()
    {
        // Arrange
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", 5000, 3000);
        fechamento.Reabrir(); // Primeiro reabrir funciona

        // Act & Assert - Segundo reabrir deve falhar
        var act = () => fechamento.Reabrir();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Só é possível reabrir um mês fechado*");
    }

    [Theory]
    [InlineData(10000, 5000, 5000)]
    [InlineData(0, 0, 0)]
    [InlineData(1000, 2000, -1000)]
    [InlineData(5500.50, 3200.75, 2299.75)]
    public void CriarFechamentoMensal_DeveCalcularSaldoFinalCorretamente(
        decimal receitas, decimal despesas, decimal saldoEsperado)
    {
        // Arrange & Act
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", receitas, despesas);

        // Assert
        fechamento.SaldoFinal.Should().Be(saldoEsperado);
    }

    [Theory]
    [InlineData("2025-01")]
    [InlineData("2024-12")]
    [InlineData("2023-06")]
    [InlineData("2025-10")]
    public void CriarFechamentoMensal_DeveAceitarDiferentesFormatosAnoMes(string anoMes)
    {
        // Arrange & Act
        var fechamento = new FechamentoMensal(_usuarioId, anoMes, 1000, 500);

        // Assert
        fechamento.AnoMes.Should().Be(anoMes);
    }

    [Fact]
    public void FechamentoMensal_DeveSerFechadoPorPadrao()
    {
        // Arrange & Act
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", 1000, 500);

        // Assert
        fechamento.Status.Should().Be(StatusFechamento.Fechado);
    }

    [Fact]
    public void FechamentoMensal_DataFechamentoDeveSerUTC()
    {
        // Arrange & Act
        var fechamento = new FechamentoMensal(_usuarioId, "2025-10", 1000, 500);

        // Assert
        fechamento.DataFechamento.Kind.Should().Be(DateTimeKind.Utc);
    }
}
