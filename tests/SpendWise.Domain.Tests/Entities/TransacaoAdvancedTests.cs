using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Domain.Tests.Entities;

public class TransacaoAdvancedTests
{
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _categoriaId = Guid.NewGuid();

    [Fact]
    public void CriarTransacao_DeveDefinirDataCriacaoAutomaticamente()
    {
        // Arrange & Act
        var antes = DateTime.UtcNow;
        var transacao = new Transacao(
            "Teste",
            new Money(100m),
            DateTime.Now,
            TipoTransacao.Despesa,
            _categoriaId,
            _usuarioId
        );
        var depois = DateTime.UtcNow;

        // Assert
        transacao.CreatedAt.Should().BeOnOrAfter(antes);
        transacao.CreatedAt.Should().BeOnOrBefore(depois);
    }

    // Teste removido - método AtualizarDescricao não existe

    [Theory]
    [InlineData(TipoTransacao.Despesa)]
    [InlineData(TipoTransacao.Receita)]
    public void CriarTransacao_DeveAceitarDiferentesTipos(TipoTransacao tipo)
    {
        // Arrange & Act
        var transacao = new Transacao(
            "Teste",
            new Money(100m),
            DateTime.Now,
            tipo,
            _categoriaId,
            _usuarioId
        );

        // Assert
        transacao.Tipo.Should().Be(tipo);
    }

    [Fact]
    public void Transacao_DeveManterIdsCorretos()
    {
        // Arrange & Act
        var transacao = new Transacao(
            "Teste",
            new Money(100m),
            DateTime.Now,
            TipoTransacao.Despesa,
            _usuarioId,      // usuarioId primeiro
            _categoriaId     // categoriaId depois
        );

        // Assert
        transacao.UsuarioId.Should().Be(_usuarioId);
        transacao.CategoriaId.Should().Be(_categoriaId);
    }

    // Teste removido - propriedade IsDeleted não existe

    [Theory]
    [InlineData("Compra supermercado")]
    [InlineData("Salário mensal")]
    [InlineData("Pagamento de conta")]
    public void CriarTransacao_DeveAceitarDiferentesDescricoes(string descricao)
    {
        // Arrange & Act
        var transacao = new Transacao(
            descricao,
            new Money(100m),
            DateTime.Now,
            TipoTransacao.Despesa,
            _categoriaId,
            _usuarioId
        );

        // Assert
        transacao.Descricao.Should().Be(descricao);
    }

    [Fact]
    public void Transacao_DeveManterDataTransacaoCorreta()
    {
        // Arrange
        var dataEsperada = new DateTime(2025, 10, 15);

        // Act
        var transacao = new Transacao(
            "Teste",
            new Money(100m),
            dataEsperada,
            TipoTransacao.Despesa,
            _categoriaId,
            _usuarioId
        );

        // Assert
        transacao.DataTransacao.Should().Be(dataEsperada);
    }
}
