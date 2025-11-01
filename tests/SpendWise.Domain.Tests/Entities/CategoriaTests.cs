using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;

namespace SpendWise.Domain.Tests.Entities;

public class CategoriaTests
{
    private readonly Guid _usuarioId = Guid.NewGuid();

    [Fact]
    public void Categoria_ShouldCreate_WithValidParameters()
    {
        // Arrange
        var nome = "Alimentação";
        var tipo = TipoCategoria.Despesa;
        var descricao = "Gastos com alimentação";

        // Act
        var categoria = new Categoria(nome, tipo, _usuarioId, descricao);

        // Assert
        categoria.Nome.Should().Be(nome);
        categoria.Tipo.Should().Be(tipo);
        categoria.UsuarioId.Should().Be(_usuarioId);
        categoria.Descricao.Should().Be(descricao);
        categoria.IsAtiva.Should().BeTrue();
        categoria.Id.Should().NotBe(Guid.Empty);
        categoria.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        categoria.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Categoria_ShouldCreate_WithoutDescricao()
    {
        // Arrange
        var nome = "Receitas";
        var tipo = TipoCategoria.Receita;

        // Act
        var categoria = new Categoria(nome, tipo, _usuarioId);

        // Assert
        categoria.Nome.Should().Be(nome);
        categoria.Tipo.Should().Be(tipo);
        categoria.UsuarioId.Should().Be(_usuarioId);
        categoria.Descricao.Should().BeNull();
        categoria.IsAtiva.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Categoria_ShouldThrowException_WithInvalidNome(string invalidNome)
    {
        // Arrange
        var tipo = TipoCategoria.Despesa;

        // Act & Assert
        Action act = () => new Categoria(invalidNome, tipo, _usuarioId);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Nome da categoria não pode ser vazio*")
           .WithParameterName("nome");
    }

    [Fact]
    public void Categoria_ShouldThrowException_WithEmptyUsuarioId()
    {
        // Arrange
        var nome = "Categoria Teste";
        var tipo = TipoCategoria.Despesa;
        var usuarioId = Guid.Empty;

        // Act & Assert
        Action act = () => new Categoria(nome, tipo, usuarioId);
        act.Should().Throw<ArgumentException>()
           .WithMessage("UsuarioId não pode ser vazio*")
           .WithParameterName("usuarioId");
    }

    [Theory]
    [InlineData(TipoCategoria.Receita)]
    [InlineData(TipoCategoria.Despesa)]
    public void Categoria_ShouldAcceptBothTipoCategoria(TipoCategoria tipo)
    {
        // Arrange & Act
        var categoria = new Categoria("Categoria Teste", tipo, _usuarioId);

        // Assert
        categoria.Tipo.Should().Be(tipo);
    }

    [Fact]
    public void Categoria_AtualizarNome_ShouldUpdateNome()
    {
        // Arrange
        var categoria = CreateValidCategoria();
        var novoNome = "Alimentação Atualizada";

        // Act
        categoria.AtualizarNome(novoNome);

        // Assert
        categoria.Nome.Should().Be(novoNome);
        categoria.UpdatedAt.Should().NotBeNull();
        categoria.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Categoria_AtualizarNome_ShouldThrowException_WithInvalidNome(string invalidNome)
    {
        // Arrange
        var categoria = CreateValidCategoria();

        // Act & Assert
        Action act = () => categoria.AtualizarNome(invalidNome);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Nome da categoria não pode ser vazio*")
           .WithParameterName("nome");
    }

    [Fact]
    public void Categoria_AtualizarDescricao_ShouldUpdateDescricao()
    {
        // Arrange
        var categoria = CreateValidCategoria();
        var novaDescricao = "Nova descrição atualizada";

        // Act
        categoria.AtualizarDescricao(novaDescricao);

        // Assert
        categoria.Descricao.Should().Be(novaDescricao);
        categoria.UpdatedAt.Should().NotBeNull();
        categoria.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Categoria_AtualizarDescricao_ShouldAcceptNull()
    {
        // Arrange
        var categoria = CreateValidCategoria();

        // Act
        categoria.AtualizarDescricao(null);

        // Assert
        categoria.Descricao.Should().BeNull();
        categoria.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Categoria_Desativar_ShouldSetIsAtivaToFalse()
    {
        // Arrange
        var categoria = CreateValidCategoria();
        categoria.IsAtiva.Should().BeTrue();

        // Act
        categoria.Desativar();

        // Assert
        categoria.IsAtiva.Should().BeFalse();
        categoria.UpdatedAt.Should().NotBeNull();
        categoria.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Categoria_Ativar_ShouldSetIsAtivaToTrue()
    {
        // Arrange
        var categoria = CreateValidCategoria();
        categoria.Desativar(); // Primeiro desativa
        categoria.IsAtiva.Should().BeFalse();

        // Act
        categoria.Ativar();

        // Assert
        categoria.IsAtiva.Should().BeTrue();
        categoria.UpdatedAt.Should().NotBeNull();
        categoria.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Categoria_ShouldInitializeEmptyTransacoesCollection()
    {
        // Arrange & Act
        var categoria = CreateValidCategoria();

        // Assert
        categoria.Transacoes.Should().NotBeNull();
        categoria.Transacoes.Should().BeEmpty();
    }

    [Fact]
    public void Categoria_ShouldMaintainImmutabilityOfCoreProperties()
    {
        // Arrange
        var categoria = CreateValidCategoria();
        var originalTipo = categoria.Tipo;
        var originalUsuarioId = categoria.UsuarioId;
        var originalId = categoria.Id;

        // Act - Tentativas de modificação através de métodos de atualização
        categoria.AtualizarNome("Novo Nome");
        categoria.AtualizarDescricao("Nova Descrição");

        // Assert - Propriedades core não devem mudar
        categoria.Tipo.Should().Be(originalTipo);
        categoria.UsuarioId.Should().Be(originalUsuarioId);
        categoria.Id.Should().Be(originalId);
    }

    [Fact]
    public void Categoria_ShouldHandleMultipleUpdates()
    {
        // Arrange
        var categoria = CreateValidCategoria();

        // Act
        categoria.AtualizarNome("Nome 1");
        var firstUpdate = categoria.UpdatedAt;
        
        Thread.Sleep(1); // Garantir diferença no timestamp
        
        categoria.AtualizarNome("Nome 2");
        var secondUpdate = categoria.UpdatedAt;

        // Assert
        categoria.Nome.Should().Be("Nome 2");
        secondUpdate.Should().BeAfter(firstUpdate!.Value);
    }

    [Fact]
    public void Categoria_ShouldCreateCategoriaReceita()
    {
        // Arrange & Act
        var categoria = new Categoria("Salário", TipoCategoria.Receita, _usuarioId, "Receitas de salário");

        // Assert
        categoria.Nome.Should().Be("Salário");
        categoria.Tipo.Should().Be(TipoCategoria.Receita);
        categoria.Descricao.Should().Be("Receitas de salário");
    }

    [Fact]
    public void Categoria_ShouldCreateCategoriaDespesa()
    {
        // Arrange & Act
        var categoria = new Categoria("Lazer", TipoCategoria.Despesa, _usuarioId, "Gastos com entretenimento");

        // Assert
        categoria.Nome.Should().Be("Lazer");
        categoria.Tipo.Should().Be(TipoCategoria.Despesa);
        categoria.Descricao.Should().Be("Gastos com entretenimento");
    }

    [Fact]
    public void Categoria_ShouldAllowEmptyStringAsDescricao()
    {
        // Arrange & Act
        var categoria = new Categoria("Teste", TipoCategoria.Despesa, _usuarioId, "");

        // Assert
        categoria.Descricao.Should().Be("");
    }

    [Fact]
    public void Categoria_ShouldAllowWhitespaceAsDescricao()
    {
        // Arrange & Act
        var categoria = new Categoria("Teste", TipoCategoria.Despesa, _usuarioId, "   ");

        // Assert
        categoria.Descricao.Should().Be("   ");
    }

    [Fact]
    public void Categoria_ShouldPreserveOriginalState_WhenUpdateFails()
    {
        // Arrange
        var categoria = CreateValidCategoria();
        var originalNome = categoria.Nome;
        var originalUpdatedAt = categoria.UpdatedAt;

        // Act & Assert
        Action act = () => categoria.AtualizarNome(""); // Vai falhar
        act.Should().Throw<ArgumentException>();

        // Estado original deve ser preservado
        categoria.Nome.Should().Be(originalNome);
        categoria.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    private Categoria CreateValidCategoria()
    {
        return new Categoria(
            "Alimentação",
            TipoCategoria.Despesa,
            _usuarioId,
            "Gastos com alimentação");
    }
}
