using FluentAssertions;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("usuario@exemplo.com")]
    [InlineData("teste.email@dominio.com.br")]
    [InlineData("user123@test-domain.org")]
    [InlineData("name.surname+tag@domain.co.uk")]
    public void Email_ShouldCreate_WithValidEmailAddresses(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Valor.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("USUARIO@EXEMPLO.COM")]
    [InlineData("Teste.Email@Dominio.COM.BR")]
    public void Email_ShouldConvertToLowerCase(string emailInput)
    {
        // Act
        var email = new Email(emailInput);

        // Assert
        email.Valor.Should().Be(emailInput.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user.domain.com")]
    [InlineData("user@domain")]
    [InlineData("user space@domain.com")]
    public void Email_ShouldThrowException_WithInvalidEmailAddresses(string invalidEmail)
    {
        // Act & Assert
        Action act = () => new Email(invalidEmail);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Email inválido*");
    }

    [Fact]
    public void Email_Equals_ShouldReturnTrue_WhenEmailsAreEqual()
    {
        // Arrange
        var email1 = new Email("teste@exemplo.com");
        var email2 = new Email("TESTE@EXEMPLO.COM");

        // Act & Assert
        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Email_Equals_ShouldReturnFalse_WhenEmailsAreDifferent()
    {
        // Arrange
        var email1 = new Email("teste1@exemplo.com");
        var email2 = new Email("teste2@exemplo.com");

        // Act & Assert
        email1.Equals(email2).Should().BeFalse();
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void Email_ImplicitConversion_ShouldConvertToString()
    {
        // Arrange
        var email = new Email("teste@exemplo.com");

        // Act
        string valor = email;

        // Assert
        valor.Should().Be("teste@exemplo.com");
    }

    [Fact]
    public void Email_ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = new Email("usuario@dominio.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("usuario@dominio.com");
    }

    [Fact]
    public void Email_GetHashCode_ShouldBeSame_WhenEmailsAreEqual()
    {
        // Arrange
        var email1 = new Email("teste@exemplo.com");
        var email2 = new Email("TESTE@EXEMPLO.COM");

        // Act & Assert
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Theory]
    [InlineData("usuario@gmail.com", "gmail.com")]
    [InlineData("teste@empresa.com.br", "empresa.com.br")]
    [InlineData("admin@sub.domain.org", "sub.domain.org")]
    public void Email_Domain_ShouldExtractDomainCorrectly(string emailAddress, string expectedDomain)
    {
        // Arrange
        var email = new Email(emailAddress);

        // Act
        var domain = email.Domain;

        // Assert
        domain.Should().Be(expectedDomain);
    }

    [Theory]
    [InlineData("usuario@gmail.com", "usuario")]
    [InlineData("teste.nome@empresa.com", "teste.nome")]
    [InlineData("admin+tag@domain.org", "admin+tag")]
    public void Email_LocalPart_ShouldExtractLocalPartCorrectly(string emailAddress, string expectedLocalPart)
    {
        // Arrange
        var email = new Email(emailAddress);

        // Act
        var localPart = email.LocalPart;

        // Assert
        localPart.Should().Be(expectedLocalPart);
    }
}
