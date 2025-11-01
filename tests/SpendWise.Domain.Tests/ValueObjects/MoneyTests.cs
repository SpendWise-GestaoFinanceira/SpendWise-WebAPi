using FluentAssertions;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Money_ShouldCreateWithValidValues()
    {
        // Arrange
        var valor = 100.50m;
        var moeda = "BRL";

        // Act
        var money = new Money(valor, moeda);

        // Assert
        money.Valor.Should().Be(100.50m);
        money.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void Money_ShouldRoundToTwoDecimalPlaces()
    {
        // Arrange
        var valor = 100.123456m;

        // Act
        var money = new Money(valor);

        // Assert
        money.Valor.Should().Be(100.12m);
    }

    [Fact]
    public void Money_ShouldUseDefaultCurrencyAsBRL()
    {
        // Arrange & Act
        var money = new Money(100);

        // Assert
        money.Moeda.Should().Be("BRL");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Money_ShouldThrowException_WhenCurrencyIsInvalid(string invalidCurrency)
    {
        // Arrange & Act & Assert
        Action act = () => new Money(100, invalidCurrency);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Moeda não pode ser vazia*");
    }

    [Fact]
    public void Money_ShouldConvertCurrencyToUpperCase()
    {
        // Arrange & Act
        var money = new Money(100, "usd");

        // Assert
        money.Moeda.Should().Be("USD");
    }

    [Fact]
    public void Money_Add_ShouldSumTwoMoneyWithSameCurrency()
    {
        // Arrange
        var money1 = new Money(100, "BRL");
        var money2 = new Money(50, "BRL");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Valor.Should().Be(150);
        result.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void Money_Add_ShouldThrowException_WhenCurrenciesAreDifferent()
    {
        // Arrange
        var money1 = new Money(100, "BRL");
        var money2 = new Money(50, "USD");

        // Act & Assert
        Action act = () => money1.Add(money2);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível somar valores de moedas diferentes");
    }

    [Fact]
    public void Money_Subtract_ShouldSubtractTwoMoneyWithSameCurrency()
    {
        // Arrange
        var money1 = new Money(100, "BRL");
        var money2 = new Money(30, "BRL");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Valor.Should().Be(70);
        result.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void Money_Subtract_ShouldThrowException_WhenCurrenciesAreDifferent()
    {
        // Arrange
        var money1 = new Money(100, "BRL");
        var money2 = new Money(30, "USD");

        // Act & Assert
        Action act = () => money1.Subtract(money2);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível subtrair valores de moedas diferentes");
    }

    [Fact]
    public void Money_Multiply_ShouldMultiplyByDecimal()
    {
        // Arrange
        var money = new Money(100, "BRL");
        var multiplier = 2.5m;

        // Act
        var result = money.Multiply(multiplier);

        // Assert
        result.Valor.Should().Be(250);
        result.Moeda.Should().Be("BRL");
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(100, false)]
    [InlineData(-50, false)]
    public void Money_IsZero_ShouldReturnCorrectValue(decimal valor, bool expected)
    {
        // Arrange
        var money = new Money(valor);

        // Act & Assert
        money.IsZero.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-50, false)]
    public void Money_IsPositive_ShouldReturnCorrectValue(decimal valor, bool expected)
    {
        // Arrange
        var money = new Money(valor);

        // Act & Assert
        money.IsPositive.Should().Be(expected);
    }

    [Theory]
    [InlineData(-50, true)]
    [InlineData(0, false)]
    [InlineData(100, false)]
    public void Money_IsNegative_ShouldReturnCorrectValue(decimal valor, bool expected)
    {
        // Arrange
        var money = new Money(valor);

        // Act & Assert
        money.IsNegative.Should().Be(expected);
    }

    [Fact]
    public void Money_Zero_ShouldCreateZeroMoney()
    {
        // Act
        var zero = Money.Zero();

        // Assert
        zero.Valor.Should().Be(0);
        zero.Moeda.Should().Be("BRL");
        zero.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Money_Zero_ShouldCreateZeroMoneyWithSpecificCurrency()
    {
        // Act
        var zero = Money.Zero("USD");

        // Assert
        zero.Valor.Should().Be(0);
        zero.Moeda.Should().Be("USD");
    }

    [Fact]
    public void Money_ImplicitConversion_ShouldConvertToDecimal()
    {
        // Arrange
        var money = new Money(150.75m);

        // Act
        decimal valor = money;

        // Assert
        valor.Should().Be(150.75m);
    }

    [Fact]
    public void Money_Equals_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var money1 = new Money(100, "BRL");
        var money2 = new Money(100, "BRL");

        // Act & Assert
        money1.Equals(money2).Should().BeTrue();
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Money_Equals_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        // Arrange
        var money1 = new Money(100, "BRL");
        var money2 = new Money(150, "BRL");

        // Act & Assert
        money1.Equals(money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Money_Equals_ShouldReturnFalse_WhenCurrenciesAreDifferent()
    {
        // Arrange
        var money1 = new Money(100, "BRL");
        var money2 = new Money(100, "USD");

        // Act & Assert
        money1.Equals(money2).Should().BeFalse();
    }

    [Fact]
    public void Money_ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var money = new Money(1234.56m, "BRL");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("1.234,56").And.Contain("BRL");
    }

    [Fact]
    public void Money_GetHashCode_ShouldBeSame_WhenValuesAreEqual()
    {
        // Arrange
        var money1 = new Money(100, "BRL");
        var money2 = new Money(100, "BRL");

        // Act & Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }
}
