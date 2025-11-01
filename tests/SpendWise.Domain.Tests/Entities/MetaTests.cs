using FluentAssertions;
using SpendWise.Domain.Entities;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Domain.Tests.Entities;

public class MetaTests
{
    private readonly Guid _usuarioId = Guid.NewGuid();

    [Fact]
    public void CriarMeta_DeveInicializarCorretamente()
    {
        // Arrange
        var nome = "Viagem para Europa";
        var descricao = "Economizar para viagem de férias";
        var valorObjetivo = new Money(10000m);
        var prazo = DateTime.UtcNow.AddMonths(12);

        // Act
        var meta = new Meta(nome, descricao, valorObjetivo, prazo, _usuarioId);

        // Assert
        meta.Nome.Should().Be(nome);
        meta.Descricao.Should().Be(descricao);
        meta.ValorObjetivo.Should().Be(valorObjetivo);
        meta.Prazo.Should().Be(prazo);
        meta.UsuarioId.Should().Be(_usuarioId);
        meta.ValorAtual.Valor.Should().Be(0);
        meta.IsAtiva.Should().BeTrue();
        meta.DataAlcancada.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarMeta_DeveLancarException_QuandoNomeInvalido(string nomeInvalido)
    {
        // Arrange & Act & Assert
        var act = () => new Meta(nomeInvalido, "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        act.Should().Throw<ArgumentException>().WithMessage("*Nome*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarMeta_DeveLancarException_QuandoDescricaoInvalida(string descricaoInvalida)
    {
        // Arrange & Act & Assert
        var act = () => new Meta("Nome", descricaoInvalida, new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        act.Should().Throw<ArgumentException>().WithMessage("*Descrição*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void CriarMeta_DeveLancarException_QuandoValorObjetivoInvalido(decimal valor)
    {
        // Arrange & Act & Assert
        var act = () => new Meta("Nome", "Descrição", new Money(valor), DateTime.UtcNow.AddDays(30), _usuarioId);
        act.Should().Throw<ArgumentException>().WithMessage("*Valor objetivo*");
    }

    [Fact]
    public void CriarMeta_DeveLancarException_QuandoPrazoNoPassado()
    {
        // Arrange & Act & Assert
        var act = () => new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(-1), _usuarioId);
        act.Should().Throw<ArgumentException>().WithMessage("*Prazo*");
    }

    [Fact]
    public void AtualizarNome_DeveAlterarNome()
    {
        // Arrange
        var meta = new Meta("Nome Original", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        var novoNome = "Nome Atualizado";

        // Act
        meta.AtualizarNome(novoNome);

        // Assert
        meta.Nome.Should().Be(novoNome);
        meta.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AtualizarDescricao_DeveAlterarDescricao()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição Original", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        var novaDescricao = "Descrição Atualizada";

        // Act
        meta.AtualizarDescricao(novaDescricao);

        // Assert
        meta.Descricao.Should().Be(novaDescricao);
    }

    [Fact]
    public void AdicionarProgresso_DeveAumentarValorAtual()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        var progresso = new Money(250);

        // Act
        meta.AdicionarProgresso(progresso);

        // Assert
        meta.ValorAtual.Valor.Should().Be(250);
    }

    [Fact]
    public void AdicionarProgresso_DeveMarcarDataAlcancada_QuandoAtingeObjetivo()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);

        // Act
        meta.AdicionarProgresso(new Money(1000));

        // Assert
        meta.DataAlcancada.Should().NotBeNull();
        meta.DataAlcancada.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RemoverProgresso_DeveDiminuirValorAtual()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.AdicionarProgresso(new Money(500));

        // Act
        meta.RemoverProgresso(new Money(200));

        // Assert
        meta.ValorAtual.Valor.Should().Be(300);
    }

    [Fact]
    public void RemoverProgresso_DeveResetarDataAlcancada_QuandoValorDiminui()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.AdicionarProgresso(new Money(1000)); // Alcança meta
        meta.DataAlcancada.Should().NotBeNull();

        // Act
        meta.RemoverProgresso(new Money(100));

        // Assert
        meta.DataAlcancada.Should().BeNull();
    }

    [Fact]
    public void CalcularPercentualProgresso_DeveRetornarValorCorreto()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.AdicionarProgresso(new Money(250));

        // Act
        var percentual = meta.CalcularPercentualProgresso();

        // Assert
        percentual.Should().Be(25);
    }

    [Fact]
    public void CalcularPercentualProgresso_NaoDeveUltrapassar100()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.AdicionarProgresso(new Money(1500));

        // Act
        var percentual = meta.CalcularPercentualProgresso();

        // Assert
        percentual.Should().Be(100);
    }

    [Fact]
    public void CalcularValorRestante_DeveRetornarDiferencaCorreta()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.AdicionarProgresso(new Money(300));

        // Act
        var restante = meta.CalcularValorRestante();

        // Assert
        restante.Valor.Should().Be(700);
    }

    [Fact]
    public void CalcularValorRestante_NaoDeveSerNegativo()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.AdicionarProgresso(new Money(1200));

        // Act
        var restante = meta.CalcularValorRestante();

        // Assert
        restante.Valor.Should().Be(0);
    }

    [Fact]
    public void Desativar_DeveMarcarMetaComoInativa()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);

        // Act
        meta.Desativar();

        // Assert
        meta.IsAtiva.Should().BeFalse();
    }

    [Fact]
    public void Reativar_DeveMarcarMetaComoAtiva()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.Desativar();

        // Act
        meta.Reativar();

        // Assert
        meta.IsAtiva.Should().BeTrue();
    }

    [Fact]
    public void ObterStatusDescricao_DeveRetornarInativa_QuandoDesativada()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.Desativar();

        // Act
        var status = meta.ObterStatusDescricao();

        // Assert
        status.Should().Be("Inativa");
    }

    [Fact]
    public void ObterStatusDescricao_DeveRetornarAlcancada_QuandoAtingida()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);
        meta.AdicionarProgresso(new Money(1000));

        // Act
        var status = meta.ObterStatusDescricao();

        // Assert
        status.Should().Be("Alcançada");
    }

    [Fact]
    public void ObterStatusDescricao_DeveRetornarEmProgresso_QuandoAtiva()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);

        // Act
        var status = meta.ObterStatusDescricao();

        // Assert
        status.Should().Be("Em progresso");
    }

    [Fact]
    public void ProjetarDataAlcance_DeveCalcularCorretamente()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1200), DateTime.UtcNow.AddMonths(12), _usuarioId);
        meta.AdicionarProgresso(new Money(200));
        var mediaEconomiaMensal = 100m;

        // Act
        var dataProjetada = meta.ProjetarDataAlcance(mediaEconomiaMensal);

        // Assert
        dataProjetada.Should().NotBeNull();
        dataProjetada.Value.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void ProjetarDataAlcance_DeveRetornarNull_QuandoMediaZero()
    {
        // Arrange
        var meta = new Meta("Nome", "Descrição", new Money(1000), DateTime.UtcNow.AddDays(30), _usuarioId);

        // Act
        var dataProjetada = meta.ProjetarDataAlcance(0);

        // Assert
        dataProjetada.Should().BeNull();
    }
}
