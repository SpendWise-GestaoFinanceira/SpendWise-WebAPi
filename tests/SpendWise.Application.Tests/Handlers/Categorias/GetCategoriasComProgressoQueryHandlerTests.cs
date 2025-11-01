using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.Categorias;
using SpendWise.Application.Queries.Categorias;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Categorias;

public class GetCategoriasComProgressoQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GetCategoriasComProgressoQueryHandler>> _loggerMock;
    private readonly GetCategoriasComProgressoQueryHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public GetCategoriasComProgressoQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<GetCategoriasComProgressoQueryHandler>>();

        _unitOfWorkMock.Setup(u => u.Categorias).Returns(_categoriaRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);

        _handler = new GetCategoriasComProgressoQueryHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_DeveRetornarCategoriasComProgresso()
    {
        // Arrange
        var categoria = new Categoria("Alimentação", TipoCategoria.Despesa, _usuarioId, null, new Money(1000));
        var categorias = new List<Categoria> { categoria };

        var categoriaDto = new CategoriaComProgressoDto
        {
            Id = categoria.Id,
            Nome = "Alimentação",
            Limite = new Money(1000)
        };

        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(categorias);

        _mapperMock
            .Setup(m => m.Map<CategoriaComProgressoDto>(categoria))
            .Returns(categoriaDto);

        _transacaoRepositoryMock
            .Setup(r => r.BuscarPorPeriodoComCategoriasAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Transacao>());

        var query = new GetCategoriasComProgressoQuery(_usuarioId, DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        _categoriaRepositoryMock.Verify(r => r.GetByUsuarioIdAsync(_usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarListaVazia_QuandoNaoHaCategorias()
    {
        // Arrange
        _categoriaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Categoria>());

        var query = new GetCategoriasComProgressoQuery(_usuarioId, DateTime.Now);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
