using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Tests.Entities;

public class TransacaoTests
{
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _categoriaId = Guid.NewGuid();

    [Fact]
    public void Transacao_ShouldCreate_WithValidParameters()
    {
        // Arrange
        var descricao = "Compra no supermercado";
        var valor = new Money(150.75m);
        var dataTransacao = DateTime.Today;
        var tipo = TipoTransacao.Despesa;
        var observacoes = "Compras mensais";

        // Act
        var transacao = new Transacao(
            descricao, 
            valor, 
            dataTransacao, 
            tipo, 
            _usuarioId, 
            _categoriaId, 
            observacoes);

        // Assert
        transacao.Descricao.Should().Be(descricao);
        transacao.Valor.Should().Be(valor);
        transacao.DataTransacao.Should().Be(dataTransacao);
        transacao.Tipo.Should().Be(tipo);
        transacao.UsuarioId.Should().Be(_usuarioId);
        transacao.CategoriaId.Should().Be(_categoriaId);
        transacao.Observacoes.Should().Be(observacoes);
        transacao.Id.Should().NotBe(Guid.Empty);
        transacao.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        transacao.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Transacao_ShouldCreateReceita_WithValidParameters()
    {
        // Arrange
        var descricao = "Salário mensal";
        var valor = new Money(5000m);
        var dataTransacao = DateTime.Today.AddDays(-1);
        var tipo = TipoTransacao.Receita;

        // Act
        var transacao = new Transacao(
            descricao, 
            valor, 
            dataTransacao, 
            tipo, 
            _usuarioId, 
            _categoriaId);

        // Assert
        transacao.Descricao.Should().Be(descricao);
        transacao.Valor.Should().Be(valor);
        transacao.DataTransacao.Should().Be(dataTransacao);
        transacao.Tipo.Should().Be(TipoTransacao.Receita);
        transacao.UsuarioId.Should().Be(_usuarioId);
        transacao.CategoriaId.Should().Be(_categoriaId);
        transacao.Observacoes.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Transacao_ShouldThrowException_WithInvalidDescricao(string invalidDescricao)
    {
        // Arrange
        var valor = new Money(100m);
        var dataTransacao = DateTime.Today;
        var tipo = TipoTransacao.Despesa;

        // Act & Assert
        Action act = () => new Transacao(
            invalidDescricao, 
            valor, 
            dataTransacao, 
            tipo, 
            _usuarioId, 
            _categoriaId);

        act.Should().Throw<ArgumentException>()
           .WithMessage("Descrição não pode ser vazia*")
           .WithParameterName("descricao");
    }

    [Fact]
    public void Transacao_ShouldThrowException_WithNullValor()
    {
        // Arrange
        var descricao = "Teste transacao";
        Money? valor = null;
        var dataTransacao = DateTime.Today;
        var tipo = TipoTransacao.Despesa;

        // Act & Assert
        Action act = () => new Transacao(
            descricao, 
            valor!, 
            dataTransacao, 
            tipo, 
            _usuarioId, 
            _categoriaId);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("valor");
    }

    [Fact]
    public void Transacao_ShouldThrowException_WithEmptyUsuarioId()
    {
        // Arrange
        var descricao = "Teste transacao";
        var valor = new Money(100m);
        var dataTransacao = DateTime.Today;
        var tipo = TipoTransacao.Despesa;
        var usuarioId = Guid.Empty;

        // Act & Assert
        Action act = () => new Transacao(
            descricao, 
            valor, 
            dataTransacao, 
            tipo, 
            usuarioId, 
            _categoriaId);

        act.Should().Throw<ArgumentException>()
           .WithMessage("UsuarioId não pode ser vazio*")
           .WithParameterName("usuarioId");
    }

    [Fact]
    public void Transacao_ShouldThrowException_WithEmptyCategoriaId()
    {
        // Arrange
        var descricao = "Teste transacao";
        var valor = new Money(100m);
        var dataTransacao = DateTime.Today;
        var tipo = TipoTransacao.Despesa;
        var categoriaId = Guid.Empty;

        // Act & Assert
        Action act = () => new Transacao(
            descricao, 
            valor, 
            dataTransacao, 
            tipo, 
            _usuarioId, 
            categoriaId);

        act.Should().Throw<ArgumentException>()
           .WithMessage("CategoriaId não pode ser vazio*")
           .WithParameterName("categoriaId");
    }

    [Fact]
    public void Transacao_AtualizarDescricao_ShouldUpdateDescricao()
    {
        // Arrange
        var transacao = CreateValidTransacao();
        var novaDescricao = "Nova descrição atualizada";

        // Act
        transacao.AtualizarDescricao(novaDescricao);

        // Assert
        transacao.Descricao.Should().Be(novaDescricao);
        transacao.UpdatedAt.Should().NotBeNull();
        transacao.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Transacao_AtualizarDescricao_ShouldThrowException_WithInvalidDescricao(string invalidDescricao)
    {
        // Arrange
        var transacao = CreateValidTransacao();

        // Act & Assert
        Action act = () => transacao.AtualizarDescricao(invalidDescricao);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Descrição não pode ser vazia*")
           .WithParameterName("descricao");
    }

    [Fact]
    public void Transacao_AtualizarValor_ShouldUpdateValor()
    {
        // Arrange
        var transacao = CreateValidTransacao();
        var novoValor = new Money(250.50m);

        // Act
        transacao.AtualizarValor(novoValor);

        // Assert
        transacao.Valor.Should().Be(novoValor);
        transacao.UpdatedAt.Should().NotBeNull();
        transacao.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Transacao_AtualizarValor_ShouldThrowException_WithNullValor()
    {
        // Arrange
        var transacao = CreateValidTransacao();
        Money? novoValor = null;

        // Act & Assert
        Action act = () => transacao.AtualizarValor(novoValor!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("valor");
    }

    [Fact]
    public void Transacao_AtualizarDataTransacao_ShouldUpdateData()
    {
        // Arrange
        var transacao = CreateValidTransacao();
        var novaData = DateTime.Today.AddDays(-5);

        // Act
        transacao.AtualizarDataTransacao(novaData);

        // Assert
        transacao.DataTransacao.Should().Be(novaData);
        transacao.UpdatedAt.Should().NotBeNull();
        transacao.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Transacao_AtualizarObservacoes_ShouldUpdateObservacoes()
    {
        // Arrange
        var transacao = CreateValidTransacao();
        var novasObservacoes = "Observações atualizadas";

        // Act
        transacao.AtualizarObservacoes(novasObservacoes);

        // Assert
        transacao.Observacoes.Should().Be(novasObservacoes);
        transacao.UpdatedAt.Should().NotBeNull();
        transacao.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Transacao_AtualizarObservacoes_ShouldAcceptNull()
    {
        // Arrange
        var transacao = CreateValidTransacao();

        // Act
        transacao.AtualizarObservacoes(null);

        // Assert
        transacao.Observacoes.Should().BeNull();
        transacao.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Transacao_AtualizarCategoria_ShouldUpdateCategoria()
    {
        // Arrange
        var transacao = CreateValidTransacao();
        var novaCategoriaId = Guid.NewGuid();

        // Act
        transacao.AtualizarCategoria(novaCategoriaId);

        // Assert
        transacao.CategoriaId.Should().Be(novaCategoriaId);
        transacao.UpdatedAt.Should().NotBeNull();
        transacao.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Transacao_AtualizarCategoria_ShouldThrowException_WithEmptyCategoriaId()
    {
        // Arrange
        var transacao = CreateValidTransacao();
        var novaCategoriaId = Guid.Empty;

        // Act & Assert
        Action act = () => transacao.AtualizarCategoria(novaCategoriaId);
        act.Should().Throw<ArgumentException>()
           .WithMessage("CategoriaId não pode ser vazio*")
           .WithParameterName("categoriaId");
    }

    [Theory]
    [InlineData(TipoTransacao.Receita)]
    [InlineData(TipoTransacao.Despesa)]
    public void Transacao_ShouldAcceptBothTipoTransacao(TipoTransacao tipo)
    {
        // Arrange & Act
        var transacao = new Transacao(
            "Teste transacao",
            new Money(100m),
            DateTime.Today,
            tipo,
            _usuarioId,
            _categoriaId);

        // Assert
        transacao.Tipo.Should().Be(tipo);
    }

    [Fact]
    public void Transacao_ShouldAcceptFutureDate()
    {
        // Arrange
        var dataFutura = DateTime.Today.AddDays(1);

        // Act
        var transacao = new Transacao(
            "Transação futura",
            new Money(100m),
            dataFutura,
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId);

        // Assert
        transacao.DataTransacao.Should().Be(dataFutura);
    }

    [Fact]
    public void Transacao_ShouldAcceptPastDate()
    {
        // Arrange
        var dataPassada = DateTime.Today.AddDays(-30);

        // Act
        var transacao = new Transacao(
            "Transação passada",
            new Money(100m),
            dataPassada,
            TipoTransacao.Receita,
            _usuarioId,
            _categoriaId);

        // Assert
        transacao.DataTransacao.Should().Be(dataPassada);
    }

    [Fact]
    public void Transacao_ShouldMaintainImmutabilityOfIds()
    {
        // Arrange
        var transacao = CreateValidTransacao();
        var originalUsuarioId = transacao.UsuarioId;
        var originalId = transacao.Id;

        // Act - Tentativas de modificação através de métodos de atualização
        transacao.AtualizarDescricao("Nova descrição");
        transacao.AtualizarValor(new Money(999m));

        // Assert - IDs não devem mudar
        transacao.UsuarioId.Should().Be(originalUsuarioId);
        transacao.Id.Should().Be(originalId);
    }

    [Fact]
    public void Transacao_ShouldCreateWithDifferentValueObjectInstances()
    {
        // Arrange
        var valor1 = new Money(100m);
        var valor2 = new Money(100m); // Mesmo valor, instância diferente

        // Act
        var transacao1 = CreateValidTransacaoWithValor(valor1);
        var transacao2 = CreateValidTransacaoWithValor(valor2);

        // Assert
        transacao1.Valor.Should().Be(transacao2.Valor); // Valores iguais
        transacao1.Valor.Should().NotBeSameAs(transacao2.Valor); // Instâncias diferentes
    }

    private Transacao CreateValidTransacao()
    {
        return new Transacao(
            "Transação de teste",
            new Money(100m),
            DateTime.Today,
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId,
            "Observações de teste");
    }

    private Transacao CreateValidTransacaoWithValor(Money valor)
    {
        return new Transacao(
            "Transação de teste",
            valor,
            DateTime.Today,
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId);
    }
}
