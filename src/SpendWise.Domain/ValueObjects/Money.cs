namespace SpendWise.Domain.ValueObjects;

public class Money
{
    public decimal Valor { get; private set; }
    public string Moeda { get; private set; }

    private Money() { } // Para EF Core

    public Money(decimal valor, string moeda = "BRL")
    {
        if (string.IsNullOrWhiteSpace(moeda))
            throw new ArgumentException("Moeda não pode ser vazia", nameof(moeda));

        Valor = Math.Round(valor, 2);
        Moeda = moeda.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        if (Moeda != other.Moeda)
            throw new InvalidOperationException("Não é possível somar valores de moedas diferentes");

        return new Money(Valor + other.Valor, Moeda);
    }

    public Money Subtract(Money other)
    {
        if (Moeda != other.Moeda)
            throw new InvalidOperationException("Não é possível subtrair valores de moedas diferentes");

        return new Money(Valor - other.Valor, Moeda);
    }

    public Money Multiply(decimal multiplier)
    {
        return new Money(Valor * multiplier, Moeda);
    }

    public bool IsZero => Valor == 0;
    public bool IsPositive => Valor > 0;
    public bool IsNegative => Valor < 0;

    public static Money Zero(string moeda = "BRL") => new(0, moeda);

    public static implicit operator decimal(Money money) => money.Valor;

    public override string ToString() => $"{Valor:C} {Moeda}";

    public override bool Equals(object? obj)
    {
        if (obj is not Money other) return false;
        return Valor == other.Valor && Moeda == other.Moeda;
    }

    public override int GetHashCode() => HashCode.Combine(Valor, Moeda);

    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);
    public static bool operator >(Money left, Money right) => left.Valor > right.Valor;
    public static bool operator <(Money left, Money right) => left.Valor < right.Valor;
    public static bool operator >=(Money left, Money right) => left.Valor >= right.Valor;
    public static bool operator <=(Money left, Money right) => left.Valor <= right.Valor;
}
