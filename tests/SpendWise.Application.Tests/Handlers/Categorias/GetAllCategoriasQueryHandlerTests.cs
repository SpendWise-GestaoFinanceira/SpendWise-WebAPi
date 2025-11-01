using AutoMapper;
using FluentAssertions;
using Moq;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.Categorias;
using SpendWise.Application.Queries.Categorias;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Categorias;

public class GetAllCategoriasQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllCategoriasQueryHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public GetAllCategoriasQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock
            .Setup(u => u.Categorias)
            .Returns(_categoriaRepositoryMock.Object);

        _handler = new GetAllCategoriasQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarTodasCategorias()
    {
        // Arrange
        var categorias = new List<Categoria>
        {
            new Categoria("Alimentação", TipoCategoria.Despesa, _usuarioId),
            new Categoria("Salário", TipoCategoria.Receita, _usuarioId),
            new Categoria("Transporte", TipoCategoria.Despesa, _usuarioId)
        };

        var categoriasDto = new List<CategoriaDto>
        {
            new CategoriaDto { Nome = "Alimentação" },
            new CategoriaDto { Nome = "Salário" },
            new CategoriaDto { Nome = "Transporte" }
        };

        _categoriaRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(categorias);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<CategoriaDto>>(categorias))
            .Returns(categoriasDto);

        var query = new GetAllCategoriasQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Nome == "Alimentação");
        result.Should().Contain(c => c.Nome == "Salário");
    }

    [Fact]
    public async Task Handle_DeveRetornarListaVazia_QuandoNaoHaCategorias()
    {
        // Arrange
        _categoriaRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Categoria>());

        _mapperMock
            .Setup(m => m.Map<IEnumerable<CategoriaDto>>(It.IsAny<List<Categoria>>()))
            .Returns(new List<CategoriaDto>());

        var query = new GetAllCategoriasQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
