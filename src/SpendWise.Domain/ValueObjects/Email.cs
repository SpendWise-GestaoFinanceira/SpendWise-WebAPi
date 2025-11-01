using System.Text.RegularExpressions;

namespace SpendWise.Domain.ValueObjects;

public class Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    public string Valor { get; private set; }

    public string Domain => Valor.Split('@')[1];
    public string LocalPart => Valor.Split('@')[0];

    private Email() { } // Para EF Core

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Email inválido", nameof(valor));

        if (!EmailRegex.IsMatch(valor))
            throw new ArgumentException("Email inválido", nameof(valor));

        Valor = valor.ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Valor;
    public static implicit operator Email(string email) => new(email);

    public override string ToString() => Valor;

    public override bool Equals(object? obj)
    {
        if (obj is not Email other) return false;
        return Valor == other.Valor;
    }

    public override int GetHashCode() => Valor.GetHashCode();

    public static bool operator ==(Email left, Email right) => left.Equals(right);
    public static bool operator !=(Email left, Email right) => !left.Equals(right);
}
