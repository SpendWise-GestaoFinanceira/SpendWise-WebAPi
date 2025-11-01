using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Domain.Tests.Entities;

public class CategoriaAdvancedTests
{
    private readonly Guid _usuarioId = Guid.NewGuid();

    [Fact]
    public void AtualizarNome_DeveAlterarNome()
    {
        // Arrange
        var categoria = new Categoria("Nome Original", TipoCategoria.Despesa, _usuarioId);
        var novoNome = "Nome Atualizado";

        // Act
        categoria.AtualizarNome(novoNome);

        // Assert
        categoria.Nome.Should().Be(novoNome);
        categoria.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AtualizarNome_DeveLancarException_QuandoNomeInvalido(string nomeInvalido)
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);

        // Act & Assert
        var act = () => categoria.AtualizarNome(nomeInvalido);
        act.Should().Throw<ArgumentException>().WithMessage("*Nome*");
    }

    [Fact]
    public void AtualizarDescricao_DeveAlterarDescricao()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId, "Descrição Original");
        var novaDescricao = "Nova Descrição";

        // Act
        categoria.AtualizarDescricao(novaDescricao);

        // Assert
        categoria.Descricao.Should().Be(novaDescricao);
        categoria.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AtualizarDescricao_DevePermitirNull()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId, "Descrição");

        // Act
        categoria.AtualizarDescricao(null);

        // Assert
        categoria.Descricao.Should().BeNull();
    }

    [Fact]
    public void AtualizarLimite_DeveAlterarLimite()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);
        var novoLimite = new Money(1000);

        // Act
        categoria.AtualizarLimite(novoLimite);

        // Assert
        categoria.Limite.Should().Be(novoLimite);
        categoria.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AtualizarCor_DeveAlterarCor()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);
        var novaCor = "#FF5733";

        // Act
        categoria.AtualizarCor(novaCor);

        // Assert
        categoria.Cor.Should().Be(novaCor);
    }

    [Fact]
    public void Desativar_DeveMarcarComoInativa()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);

        // Act
        categoria.Desativar();

        // Assert
        categoria.IsAtiva.Should().BeFalse();
        categoria.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Ativar_DeveMarcarComoAtiva()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);
        categoria.Desativar();

        // Act
        categoria.Ativar();

        // Assert
        categoria.IsAtiva.Should().BeTrue();
    }

    [Fact]
    public void CalcularGastoMensal_DeveRetornarSomaCorreta()
    {
        // Arrange
        var categoria = new Categoria("Alimentação", TipoCategoria.Despesa, _usuarioId);
        var inicioMes = new DateTime(2025, 10, 1);
        var fimMes = new DateTime(2025, 10, 31);

        var transacoes = new List<Transacao>
        {
            new Transacao("Compra 1", new Money(100), inicioMes.AddDays(5), TipoTransacao.Despesa, _usuarioId, categoria.Id),
            new Transacao("Compra 2", new Money(200), inicioMes.AddDays(10), TipoTransacao.Despesa, _usuarioId, categoria.Id),
            new Transacao("Compra 3", new Money(150), inicioMes.AddDays(20), TipoTransacao.Despesa, _usuarioId, categoria.Id)
        };

        // Act
        var gasto = categoria.CalcularGastoMensal(transacoes, inicioMes, fimMes);

        // Assert
        gasto.Should().Be(450);
    }

    [Fact]
    public void CalcularGastoMensal_DeveIgnorarTransacoesForaDoPeriodo()
    {
        // Arrange
        var categoria = new Categoria("Alimentação", TipoCategoria.Despesa, _usuarioId);
        var inicioMes = new DateTime(2025, 10, 1);
        var fimMes = new DateTime(2025, 10, 31);

        var transacoes = new List<Transacao>
        {
            new Transacao("Dentro", new Money(100), inicioMes.AddDays(5), TipoTransacao.Despesa, _usuarioId, categoria.Id),
            new Transacao("Antes", new Money(200), inicioMes.AddDays(-5), TipoTransacao.Despesa, _usuarioId, categoria.Id),
            new Transacao("Depois", new Money(150), fimMes.AddDays(5), TipoTransacao.Despesa, _usuarioId, categoria.Id)
        };

        // Act
        var gasto = categoria.CalcularGastoMensal(transacoes, inicioMes, fimMes);

        // Assert
        gasto.Should().Be(100);
    }

    [Fact]
    public void CalcularPercentualUtilizado_DeveRetornarValorCorreto()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId, null, new Money(1000));
        var gastoAtual = 250m;

        // Act
        var percentual = categoria.CalcularPercentualUtilizado(gastoAtual);

        // Assert
        percentual.Should().Be(25);
    }

    [Fact]
    public void CalcularPercentualUtilizado_DeveRetornarZero_QuandoSemLimite()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);
        var gastoAtual = 500m;

        // Act
        var percentual = categoria.CalcularPercentualUtilizado(gastoAtual);

        // Assert
        percentual.Should().Be(0);
    }

    [Theory]
    [InlineData(50, StatusLimite.Normal)]
    [InlineData(850, StatusLimite.Alerta)]
    [InlineData(1100, StatusLimite.Excedido)]
    public void VerificarStatusLimite_DeveRetornarStatusCorreto(decimal gastoAtual, StatusLimite statusEsperado)
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId, null, new Money(1000));

        // Act
        var status = categoria.VerificarStatusLimite(gastoAtual);

        // Assert
        status.Should().Be(statusEsperado);
    }

    [Fact]
    public void VerificarStatusLimite_DeveRetornarSemLimite_QuandoNaoTemLimite()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);

        // Act
        var status = categoria.VerificarStatusLimite(500);

        // Assert
        status.Should().Be(StatusLimite.SemLimite);
    }

    [Fact]
    public void PodeAdicionarDespesa_DeveRetornarTrue_QuandoDentrodoLimite()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId, null, new Money(1000));
        var gastoAtual = 500m;
        var novaDespesa = new Money(300);

        // Act
        var pode = categoria.PodeAdicionarDespesa(novaDespesa, gastoAtual);

        // Assert
        pode.Should().BeTrue();
    }

    [Fact]
    public void PodeAdicionarDespesa_DeveRetornarFalse_QuandoExcederLimite()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId, null, new Money(1000));
        var gastoAtual = 900m;
        var novaDespesa = new Money(200);

        // Act
        var pode = categoria.PodeAdicionarDespesa(novaDespesa, gastoAtual);

        // Assert
        pode.Should().BeFalse();
    }

    [Fact]
    public void PodeAdicionarDespesa_DeveRetornarTrue_QuandoSemLimite()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);
        var gastoAtual = 5000m;
        var novaDespesa = new Money(1000);

        // Act
        var pode = categoria.PodeAdicionarDespesa(novaDespesa, gastoAtual);

        // Assert
        pode.Should().BeTrue();
    }

    [Theory]
    [InlineData(TipoCategoria.Receita)]
    [InlineData(TipoCategoria.Despesa)]
    public void Categoria_DeveAceitarDiferentesTipos(TipoCategoria tipo)
    {
        // Arrange & Act
        var categoria = new Categoria("Nome", tipo, _usuarioId);

        // Assert
        categoria.Tipo.Should().Be(tipo);
    }

    [Theory]
    [InlineData(PrioridadeCategoria.Essencial)]
    [InlineData(PrioridadeCategoria.Superfluo)]
    public void Categoria_DeveAceitarDiferentesPrioridades(PrioridadeCategoria prioridade)
    {
        // Arrange & Act
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId, null, null, prioridade);

        // Assert
        categoria.Prioridade.Should().Be(prioridade);
    }
}
