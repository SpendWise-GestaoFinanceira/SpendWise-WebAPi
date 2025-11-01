using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Domain.Tests.Entities;

public class UsuarioAdvancedTests
{
    [Fact]
    public void Usuario_DeveInicializarComValoresPadrao()
    {
        // Arrange & Act
        var usuario = new Usuario("João Silva", new Email("joao@teste.com"), "senha_hash");

        // Assert
        usuario.Id.Should().NotBeEmpty();
        usuario.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        usuario.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Usuario_DevePermitirAlterarNome()
    {
        // Arrange
        var usuario = new Usuario("Nome Original", new Email("teste@email.com"), "hash");
        var novoNome = "Nome Atualizado";

        // Act
        usuario.AtualizarNome(novoNome);

        // Assert
        usuario.Nome.Should().Be(novoNome);
        usuario.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AtualizarNome_DeveLancarException_QuandoNomeInvalido(string nomeInvalido)
    {
        // Arrange
        var usuario = new Usuario("Nome", new Email("teste@email.com"), "hash");

        // Act & Assert
        var act = () => usuario.AtualizarNome(nomeInvalido);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Usuario_DeveManterEmailImutavel()
    {
        // Arrange
        var email = new Email("teste@email.com");
        var usuario = new Usuario("Nome", email, "hash");

        // Assert
        usuario.Email.Should().Be(email);
        usuario.Email.Valor.Should().Be("teste@email.com");
    }

    [Fact]
    public void Usuario_DeveTerIdUnico()
    {
        // Arrange & Act
        var usuario1 = new Usuario("Nome1", new Email("email1@teste.com"), "hash");
        var usuario2 = new Usuario("Nome2", new Email("email2@teste.com"), "hash");

        // Assert
        usuario1.Id.Should().NotBe(usuario2.Id);
    }

    [Fact]
    public void AtualizarNome_DeveAtualizarUpdatedAt()
    {
        // Arrange
        var usuario = new Usuario("Nome", new Email("teste@email.com"), "hash");
        var updatedAtAnterior = usuario.UpdatedAt;

        Thread.Sleep(100);

        // Act
        usuario.AtualizarNome("Novo Nome");

        // Assert
        usuario.UpdatedAt.Should().NotBeNull();
        if (updatedAtAnterior.HasValue)
        {
            usuario.UpdatedAt.Value.Should().BeAfter(updatedAtAnterior.Value);
        }
    }

    // Nota: AtualizarSenha removido - não existe na entidade por design de segurança

    [Theory]
    [InlineData("João Silva")]
    [InlineData("Maria Santos")]
    [InlineData("Pedro Oliveira")]
    public void Usuario_DeveAceitarDiferentesNomes(string nome)
    {
        // Arrange & Act
        var usuario = new Usuario(nome, new Email("teste@email.com"), "hash");

        // Assert
        usuario.Nome.Should().Be(nome);
    }

    [Theory]
    [InlineData("teste@email.com")]
    [InlineData("usuario@dominio.com.br")]
    [InlineData("contato@empresa.org")]
    public void Usuario_DeveAceitarDiferentesEmails(string emailStr)
    {
        // Arrange
        var email = new Email(emailStr);

        // Act
        var usuario = new Usuario("Nome", email, "hash");

        // Assert
        usuario.Email.Valor.Should().Be(emailStr.ToLowerInvariant());
    }
}
