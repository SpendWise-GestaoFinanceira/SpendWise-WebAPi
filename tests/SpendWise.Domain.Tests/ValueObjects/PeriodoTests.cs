using FluentAssertions;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Domain.Tests.ValueObjects;

public class PeriodoTests
{
    [Fact]
    public void CriarPeriodo_DeveInicializarCorretamente()
    {
        // Arrange & Act
        var inicio = new DateTime(2025, 10, 1);
        var fim = new DateTime(2025, 10, 31);
        var periodo = new Periodo(inicio, fim);

        // Assert
        periodo.DataInicio.Should().Be(inicio);
        periodo.DataFim.Should().Be(fim);
    }

    [Fact]
    public void CriarPeriodo_DeveLancarException_QuandoFimAnteriorInicio()
    {
        // Arrange
        var inicio = new DateTime(2025, 10, 31);
        var fim = new DateTime(2025, 10, 1);

        // Act & Assert
        var act = () => new Periodo(inicio, fim);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*início*maior*fim*");
    }

    [Fact]
    public void CriarPeriodo_DeveAceitarPeriodoDeUmDia()
    {
        // Arrange & Act
        var data = new DateTime(2025, 10, 15);
        var periodo = new Periodo(data, data);

        // Assert
        periodo.DataInicio.Should().Be(data);
        periodo.DataFim.Should().Be(data);
    }

    [Theory]
    [InlineData("2025-10-01", "2025-10-31")]
    [InlineData("2025-01-01", "2025-12-31")]
    [InlineData("2024-06-15", "2024-06-30")]
    public void CriarPeriodo_DeveAceitarDiferentesPeriodos(string inicioStr, string fimStr)
    {
        // Arrange
        var inicio = DateTime.Parse(inicioStr);
        var fim = DateTime.Parse(fimStr);

        // Act
        var periodo = new Periodo(inicio, fim);

        // Assert
        periodo.DataInicio.Should().Be(inicio);
        periodo.DataFim.Should().Be(fim);
    }

    [Fact]
    public void Periodo_DeveSerIgual_QuandoDatasIguais()
    {
        // Arrange
        var inicio = new DateTime(2025, 10, 1);
        var fim = new DateTime(2025, 10, 31);
        var periodo1 = new Periodo(inicio, fim);
        var periodo2 = new Periodo(inicio, fim);

        // Act & Assert
        periodo1.Should().Be(periodo2);
    }

    [Fact]
    public void Periodo_NaoDeveSerIgual_QuandoDatasDiferentes()
    {
        // Arrange
        var periodo1 = new Periodo(new DateTime(2025, 10, 1), new DateTime(2025, 10, 31));
        var periodo2 = new Periodo(new DateTime(2025, 11, 1), new DateTime(2025, 11, 30));

        // Act & Assert
        periodo1.Should().NotBe(periodo2);
    }
}
