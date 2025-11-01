using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Tests.Entities;

public class UsuarioTests
{
    [Fact]
    public void Usuario_ShouldCreate_WithValidParameters()
    {
        // Arrange
        var nome = "João Silva";
        var email = new Email("joao@exemplo.com");
        var passwordHash = "hashedPassword123";
        var rendaMensal = 5000m;

        // Act
        var usuario = new Usuario(nome, email, passwordHash, rendaMensal);

        // Assert
        usuario.Nome.Should().Be(nome);
        usuario.Email.Should().Be(email);
        usuario.Senha.Should().Be(passwordHash);
        usuario.RendaMensal.Should().Be(rendaMensal);
        usuario.IsAtivo.Should().BeTrue();
        usuario.Id.Should().NotBe(Guid.Empty);
        usuario.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        usuario.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Usuario_ShouldThrowException_WithInvalidName(string invalidName)
    {
        // Arrange
        var email = new Email("joao@exemplo.com");
        var passwordHash = "hashedPassword123";
        var rendaMensal = 5000m;

        // Act & Assert
        Action act = () => new Usuario(invalidName, email, passwordHash, rendaMensal);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Nome não pode ser vazio*");
    }

    [Fact]
    public void Usuario_ShouldThrowException_WithNullEmail()
    {
        // Arrange
        var nome = "João Silva";
        Email? email = null;
        var passwordHash = "hashedPassword123";
        var rendaMensal = 5000m;

        // Act & Assert
        Action act = () => new Usuario(nome, email!, passwordHash, rendaMensal);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("email");
    }

    [Fact]
    public void Usuario_ShouldThrowException_WithNullPasswordHash()
    {
        // Arrange
        var nome = "João Silva";
        var email = new Email("joao@exemplo.com");
        string? passwordHash = null;
        var rendaMensal = 5000m;

        // Act & Assert
        Action act = () => new Usuario(nome, email, passwordHash!, rendaMensal);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("passwordHash");
    }

    [Fact]
    public void Usuario_ShouldThrowException_WithNegativeRendaMensal()
    {
        // Arrange
        var nome = "João Silva";
        var email = new Email("joao@exemplo.com");
        var passwordHash = "hashedPassword123";
        var rendaMensal = -1000m;

        // Act & Assert
        Action act = () => new Usuario(nome, email, passwordHash, rendaMensal);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Renda mensal não pode ser negativa*");
    }

    [Fact]
    public void Usuario_AtualizarNome_ShouldUpdateName()
    {
        // Arrange
        var usuario = CreateValidUsuario();
        var novoNome = "Maria Santos";

        // Act
        usuario.AtualizarNome(novoNome);

        // Assert
        usuario.Nome.Should().Be(novoNome);
        usuario.UpdatedAt.Should().NotBeNull();
        usuario.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Usuario_AtualizarNome_ShouldThrowException_WithInvalidName(string invalidName)
    {
        // Arrange
        var usuario = CreateValidUsuario();

        // Act & Assert
        Action act = () => usuario.AtualizarNome(invalidName);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Nome não pode ser vazio*");
    }

    [Fact]
    public void Usuario_AtualizarRendaMensal_ShouldUpdateRenda()
    {
        // Arrange
        var usuario = CreateValidUsuario();
        var novaRenda = 7500m;

        // Act
        usuario.AtualizarRendaMensal(novaRenda);

        // Assert
        usuario.RendaMensal.Should().Be(novaRenda);
        usuario.UpdatedAt.Should().NotBeNull();
        usuario.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Usuario_AtualizarRendaMensal_ShouldThrowException_WithNegativeValue()
    {
        // Arrange
        var usuario = CreateValidUsuario();
        var novaRenda = -500m;

        // Act & Assert
        Action act = () => usuario.AtualizarRendaMensal(novaRenda);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Renda mensal não pode ser negativa*");
    }

    [Fact]
    public void Usuario_Desativar_ShouldSetIsAtivoToFalse()
    {
        // Arrange
        var usuario = CreateValidUsuario();
        usuario.IsAtivo.Should().BeTrue();

        // Act
        usuario.Desativar();

        // Assert
        usuario.IsAtivo.Should().BeFalse();
        usuario.UpdatedAt.Should().NotBeNull();
        usuario.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Usuario_Ativar_ShouldSetIsAtivoToTrue()
    {
        // Arrange
        var usuario = CreateValidUsuario();
        usuario.Desativar(); // Primeiro desativa
        usuario.IsAtivo.Should().BeFalse();

        // Act
        usuario.Ativar();

        // Assert
        usuario.IsAtivo.Should().BeTrue();
        usuario.UpdatedAt.Should().NotBeNull();
        usuario.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Usuario_ShouldInitializeEmptyCollections()
    {
        // Arrange & Act
        var usuario = CreateValidUsuario();

        // Assert
        usuario.Categorias.Should().NotBeNull();
        usuario.Categorias.Should().BeEmpty();
        usuario.Transacoes.Should().NotBeNull();
        usuario.Transacoes.Should().BeEmpty();
    }

    [Fact]
    public void Usuario_ShouldAcceptZeroRendaMensal()
    {
        // Arrange
        var nome = "João Silva";
        var email = new Email("joao@exemplo.com");
        var passwordHash = "hashedPassword123";
        var rendaMensal = 0m;

        // Act
        var usuario = new Usuario(nome, email, passwordHash, rendaMensal);

        // Assert
        usuario.RendaMensal.Should().Be(0m);
    }

    private static Usuario CreateValidUsuario()
    {
        return new Usuario(
            "João Silva",
            new Email("joao@exemplo.com"),
            "hashedPassword123",
            5000m
        );
    }
}
