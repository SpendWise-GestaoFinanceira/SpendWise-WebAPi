using FluentAssertions;
using Moq;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.OrcamentosMensais;
using SpendWise.Application.Queries.OrcamentosMensais;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.OrcamentosMensais;

public class GetEstatisticasCategoriasQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly GetEstatisticasCategoriasQueryHandler _handler;
    private readonly Guid _usuarioId;

    public GetEstatisticasCategoriasQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();

        _unitOfWorkMock.Setup(u => u.Categorias).Returns(_categoriaRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);

        _handler = new GetEstatisticasCategoriasQueryHandler(_unitOfWorkMock.Object);
        _usuarioId = Guid.NewGuid();
    }

    [Fact]
    public async Task Handle_DeveRetornarEstatisticasVazias_QuandoNaoExistemCategorias()
    {
        // Arrange
        var query = new GetEstatisticasCategoriasQuery(_usuarioId, "2025-10");

        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Categoria>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AnoMes.Should().Be("2025-10");
        result.Categorias.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DeveCalcularPercentualCorretamente_QuandoCategoriaTemLimite()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var limite = new Money(1000);
        var categoria = new Categoria("Alimentação", TipoCategoria.Despesa, _usuarioId, limite: limite);
        typeof(Categoria).GetProperty("Id")!.SetValue(categoria, categoriaId);

        var transacao1 = new Transacao(
            descricao: "Compra 1",
            valor: new Money(300),
            dataTransacao: new DateTime(2025, 10, 5),
            tipo: TipoTransacao.Despesa,
            categoriaId: categoriaId,
            usuarioId: _usuarioId
        );

        var transacao2 = new Transacao(
            descricao: "Compra 2",
            valor: new Money(500),
            dataTransacao: new DateTime(2025, 10, 15),
            tipo: TipoTransacao.Despesa,
            categoriaId: categoriaId,
            usuarioId: _usuarioId
        );

        var query = new GetEstatisticasCategoriasQuery(_usuarioId, "2025-10");

        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Categoria> { categoria });

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(new List<Transacao> { transacao1, transacao2 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Categorias.Should().HaveCount(1);

        var categoriaResult = result.Categorias.First();
        categoriaResult.CategoriaId.Should().Be(categoriaId);
        categoriaResult.Nome.Should().Be("Alimentação");
        categoriaResult.Limite.Should().Be(1000);
        categoriaResult.Gasto.Should().Be(800);
        categoriaResult.PercentualUtilizado.Should().Be(80);
        categoriaResult.Status.Should().Be(StatusOrcamento.Atencao);
    }

    [Fact]
    public async Task Handle_DeveRetornarStatusDentro_QuandoGastoMenorQue80Porcento()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var limite = new Money(1000);
        var categoria = new Categoria("Transporte", TipoCategoria.Despesa, _usuarioId, limite: limite);
        typeof(Categoria).GetProperty("Id")!.SetValue(categoria, categoriaId);

        var transacao = new Transacao(
            descricao: "Combustível",
            valor: new Money(500),
            dataTransacao: new DateTime(2025, 10, 5),
            tipo: TipoTransacao.Despesa,
            categoriaId: categoriaId,
            usuarioId: _usuarioId
        );

        var query = new GetEstatisticasCategoriasQuery(_usuarioId, "2025-10");

        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Categoria> { categoria });

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(new List<Transacao> { transacao });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var categoriaResult = result.Categorias.First();
        categoriaResult.PercentualUtilizado.Should().Be(50);
        categoriaResult.Status.Should().Be(StatusOrcamento.Dentro);
    }

    [Fact]
    public async Task Handle_DeveRetornarStatusAlerta_QuandoGastoEntre95e100Porcento()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var limite = new Money(1000);
        var categoria = new Categoria("Lazer", TipoCategoria.Despesa, _usuarioId, limite: limite);
        typeof(Categoria).GetProperty("Id")!.SetValue(categoria, categoriaId);

        var transacao = new Transacao(
            descricao: "Cinema",
            valor: new Money(970),
            dataTransacao: new DateTime(2025, 10, 5),
            tipo: TipoTransacao.Despesa,
            categoriaId: categoriaId,
            usuarioId: _usuarioId
        );

        var query = new GetEstatisticasCategoriasQuery(_usuarioId, "2025-10");

        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Categoria> { categoria });

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(new List<Transacao> { transacao });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var categoriaResult = result.Categorias.First();
        categoriaResult.PercentualUtilizado.Should().Be(97);
        categoriaResult.Status.Should().Be(StatusOrcamento.Alerta);
    }

    [Fact]
    public async Task Handle_DeveRetornarStatusExcedido_QuandoGastoMaior100Porcento()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var limite = new Money(1000);
        var categoria = new Categoria("Moradia", TipoCategoria.Despesa, _usuarioId, limite: limite);
        typeof(Categoria).GetProperty("Id")!.SetValue(categoria, categoriaId);

        var transacao = new Transacao(
            descricao: "Aluguel",
            valor: new Money(1200),
            dataTransacao: new DateTime(2025, 10, 5),
            tipo: TipoTransacao.Despesa,
            categoriaId: categoriaId,
            usuarioId: _usuarioId
        );

        var query = new GetEstatisticasCategoriasQuery(_usuarioId, "2025-10");

        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Categoria> { categoria });

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(new List<Transacao> { transacao });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var categoriaResult = result.Categorias.First();
        categoriaResult.PercentualUtilizado.Should().Be(120);
        categoriaResult.Status.Should().Be(StatusOrcamento.Excedido);
    }

    [Fact]
    public async Task Handle_DeveIgnorarReceitas_ApenasContabilizarDespesas()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var limite = new Money(1000);
        var categoria = new Categoria("Trabalho", TipoCategoria.Despesa, _usuarioId, limite: limite);
        typeof(Categoria).GetProperty("Id")!.SetValue(categoria, categoriaId);

        var receita = new Transacao(
            descricao: "Salário",
            valor: new Money(5000),
            dataTransacao: new DateTime(2025, 10, 1),
            tipo: TipoTransacao.Receita,
            categoriaId: categoriaId,
            usuarioId: _usuarioId
        );

        var despesa = new Transacao(
            descricao: "Despesa",
            valor: new Money(200),
            dataTransacao: new DateTime(2025, 10, 5),
            tipo: TipoTransacao.Despesa,
            categoriaId: categoriaId,
            usuarioId: _usuarioId
        );

        var query = new GetEstatisticasCategoriasQuery(_usuarioId, "2025-10");

        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Categoria> { categoria });

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(new List<Transacao> { receita, despesa });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var categoriaResult = result.Categorias.First();
        categoriaResult.Gasto.Should().Be(200); // Não deve incluir a receita
        categoriaResult.PercentualUtilizado.Should().Be(20);
    }

    [Theory]
    [InlineData("2025-10")]
    [InlineData("2024-01")]
    [InlineData("2023-12")]
    public async Task Handle_DeveAceitarDiferentesFormatosAnoMes(string anoMes)
    {
        // Arrange
        var query = new GetEstatisticasCategoriasQuery(_usuarioId, anoMes);

        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Categoria>());

        _transacaoRepositoryMock
            .Setup(r => r.GetByPeriodoAsync(_usuarioId, It.IsAny<Periodo>()))
            .ReturnsAsync(new List<Transacao>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AnoMes.Should().Be(anoMes);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoAnoMesInvalido()
    {
        // Arrange
        var query = new GetEstatisticasCategoriasQuery(_usuarioId, "2025/10"); // Formato inválido

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
