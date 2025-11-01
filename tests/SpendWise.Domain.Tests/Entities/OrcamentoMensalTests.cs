using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Tests.Entities;

public class OrcamentoMensalTests
{
    private readonly Guid _usuarioId = Guid.NewGuid();

    [Fact]
    public void OrcamentoMensal_ShouldCreate_WithValidParameters()
    {
        // Arrange
        var anoMes = "2025-08";
        var valor = new Money(3000m);

        // Act
        var orcamento = new OrcamentoMensal(_usuarioId, anoMes, valor);

        // Assert
        orcamento.UsuarioId.Should().Be(_usuarioId);
        orcamento.AnoMes.Should().Be(anoMes);
        orcamento.Valor.Should().Be(valor);
        orcamento.Id.Should().NotBe(Guid.Empty);
        orcamento.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        orcamento.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void OrcamentoMensal_ShouldThrowException_WithEmptyUsuarioId()
    {
        // Arrange
        var usuarioId = Guid.Empty;
        var anoMes = "2025-08";
        var valor = new Money(3000m);

        // Act & Assert
        Action act = () => new OrcamentoMensal(usuarioId, anoMes, valor);
        act.Should().Throw<ArgumentException>()
           .WithMessage("UsuarioId não pode ser vazio*")
           .WithParameterName("usuarioId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void OrcamentoMensal_ShouldThrowException_WithInvalidAnoMes(string invalidAnoMes)
    {
        // Arrange
        var valor = new Money(3000m);

        // Act & Assert
        Action act = () => new OrcamentoMensal(_usuarioId, invalidAnoMes, valor);
        act.Should().Throw<ArgumentException>()
           .WithMessage("AnoMes não pode ser vazio*")
           .WithParameterName("anoMes");
    }

    [Theory]
    [InlineData("2025")]
    [InlineData("08-2025")]
    [InlineData("2025/08")]
    [InlineData("2025-8")]
    [InlineData("25-08")]
    [InlineData("2025-13")]
    [InlineData("2025-00")]
    [InlineData("abc-def")]
    public void OrcamentoMensal_ShouldThrowException_WithInvalidAnoMesFormat(string invalidFormat)
    {
        // Arrange
        var valor = new Money(3000m);

        // Act & Assert
        Action act = () => new OrcamentoMensal(_usuarioId, invalidFormat, valor);
        act.Should().Throw<ArgumentException>()
           .WithMessage("AnoMes deve estar no formato YYYY-MM*")
           .WithParameterName("anoMes");
    }

    [Theory]
    [InlineData("2025-01")]
    [InlineData("2025-12")]
    [InlineData("2024-06")]
    [InlineData("2026-03")]
    public void OrcamentoMensal_ShouldAccept_ValidAnoMesFormats(string validAnoMes)
    {
        // Arrange
        var valor = new Money(3000m);

        // Act
        var orcamento = new OrcamentoMensal(_usuarioId, validAnoMes, valor);

        // Assert
        orcamento.AnoMes.Should().Be(validAnoMes);
    }

    [Fact]
    public void OrcamentoMensal_ShouldThrowException_WithNullValor()
    {
        // Arrange
        var anoMes = "2025-08";
        Money? valor = null;

        // Act & Assert
        Action act = () => new OrcamentoMensal(_usuarioId, anoMes, valor!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("valor");
    }

    [Fact]
    public void OrcamentoMensal_AtualizarValor_ShouldUpdateValor()
    {
        // Arrange
        var orcamento = CreateValidOrcamento();
        var novoValor = new Money(4500m);

        // Act
        orcamento.AtualizarValor(novoValor);

        // Assert
        orcamento.Valor.Should().Be(novoValor);
        orcamento.UpdatedAt.Should().NotBeNull();
        orcamento.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void OrcamentoMensal_AtualizarValor_ShouldThrowException_WithNullValor()
    {
        // Arrange
        var orcamento = CreateValidOrcamento();
        Money? novoValor = null;

        // Act & Assert
        Action act = () => orcamento.AtualizarValor(novoValor!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("novoValor");
    }

    [Fact]
    public void OrcamentoMensal_CalcularSaldoDisponivel_ShouldReturnCorrectValue()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(3000m));
        var totalDespesas = new Money(1200m);

        // Act
        var saldoDisponivel = orcamento.CalcularSaldoDisponivel(totalDespesas);

        // Assert
        saldoDisponivel.Valor.Should().Be(1800m);
        saldoDisponivel.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void OrcamentoMensal_CalcularSaldoDisponivel_ShouldReturnNegative_WhenDespesasExceedOrcamento()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(1000m));
        var totalDespesas = new Money(1500m);

        // Act
        var saldoDisponivel = orcamento.CalcularSaldoDisponivel(totalDespesas);

        // Assert
        saldoDisponivel.Valor.Should().Be(-500m);
        saldoDisponivel.IsNegative.Should().BeTrue();
    }

    [Fact]
    public void OrcamentoMensal_CalcularPercentualUtilizado_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(2000m));
        var totalDespesas = new Money(500m);

        // Act
        var percentual = orcamento.CalcularPercentualUtilizado(totalDespesas);

        // Assert
        percentual.Should().Be(25m);
    }

    [Fact]
    public void OrcamentoMensal_CalcularPercentualUtilizado_ShouldReturnZero_WhenValorIsZero()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(0m));
        var totalDespesas = new Money(100m);

        // Act
        var percentual = orcamento.CalcularPercentualUtilizado(totalDespesas);

        // Assert
        percentual.Should().Be(0m);
    }

    [Fact]
    public void OrcamentoMensal_CalcularPercentualUtilizado_ShouldReturnOver100_WhenDespesasExceedOrcamento()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(1000m));
        var totalDespesas = new Money(1500m);

        // Act
        var percentual = orcamento.CalcularPercentualUtilizado(totalDespesas);

        // Assert
        percentual.Should().Be(150m);
    }

    [Fact]
    public void OrcamentoMensal_PodeAdicionarDespesa_ShouldReturnTrue_WhenWithinBudget()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(1000m));
        var valorDespesa = new Money(500m);

        // Act
        var podeAdicionar = orcamento.PodeAdicionarDespesa(valorDespesa);

        // Assert
        podeAdicionar.Should().BeTrue();
    }

    [Fact]
    public void OrcamentoMensal_PodeAdicionarDespesa_ShouldReturnFalse_WhenExceedsBudget()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(1000m));
        var valorDespesa = new Money(1500m);

        // Act
        var podeAdicionar = orcamento.PodeAdicionarDespesa(valorDespesa);

        // Assert
        podeAdicionar.Should().BeFalse();
    }

    [Fact]
    public void OrcamentoMensal_PodeAdicionarDespesa_ShouldReturnTrue_WhenExactlyEqualsBudget()
    {
        // Arrange
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(1000m));
        var valorDespesa = new Money(1000m);

        // Act
        var podeAdicionar = orcamento.PodeAdicionarDespesa(valorDespesa);

        // Assert
        podeAdicionar.Should().BeTrue();
    }

    [Fact]
    public void OrcamentoMensal_ObterAnoMesAtual_ShouldReturnCurrentMonthFormat()
    {
        // Act
        var anoMesAtual = OrcamentoMensal.ObterAnoMesAtual();

        // Assert
        anoMesAtual.Should().MatchRegex(@"^\d{4}-\d{2}$");
        
        // Verificar se é realmente o mês atual
        var expectedAnoMes = DateTime.Now.ToString("yyyy-MM");
        anoMesAtual.Should().Be(expectedAnoMes);
    }

    [Fact]
    public void OrcamentoMensal_ShouldMaintainImmutabilityOfCoreProperties()
    {
        // Arrange
        var orcamento = CreateValidOrcamento();
        var originalUsuarioId = orcamento.UsuarioId;
        var originalAnoMes = orcamento.AnoMes;
        var originalId = orcamento.Id;

        // Act - Tentativa de modificação através de método de atualização
        orcamento.AtualizarValor(new Money(5000m));

        // Assert - Propriedades core não devem mudar
        orcamento.UsuarioId.Should().Be(originalUsuarioId);
        orcamento.AnoMes.Should().Be(originalAnoMes);
        orcamento.Id.Should().Be(originalId);
    }

    [Fact]
    public void OrcamentoMensal_ShouldAcceptZeroValor()
    {
        // Arrange & Act
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", new Money(0m));

        // Assert
        orcamento.Valor.Valor.Should().Be(0m);
        orcamento.Valor.IsZero.Should().BeTrue();
    }

    [Fact]
    public void OrcamentoMensal_ShouldWorkWithDifferentCurrencies()
    {
        // Arrange
        var valorUSD = new Money(1000m, "USD");

        // Act
        var orcamento = new OrcamentoMensal(_usuarioId, "2025-08", valorUSD);

        // Assert
        orcamento.Valor.Moeda.Should().Be("USD");
        orcamento.Valor.Valor.Should().Be(1000m);
    }

    [Fact]
    public void OrcamentoMensal_CalcularSaldoDisponivel_ShouldWork_WithSameCurrency()
    {
        // Arrange
        var orcamentoUSD = new OrcamentoMensal(_usuarioId, "2025-08", new Money(1000m, "USD"));
        var despesasUSD = new Money(300m, "USD");

        // Act
        var saldoDisponivel = orcamentoUSD.CalcularSaldoDisponivel(despesasUSD);

        // Assert
        saldoDisponivel.Valor.Should().Be(700m);
        saldoDisponivel.Moeda.Should().Be("USD");
    }

    private OrcamentoMensal CreateValidOrcamento()
    {
        return new OrcamentoMensal(_usuarioId, "2025-08", new Money(3000m));
    }
}
