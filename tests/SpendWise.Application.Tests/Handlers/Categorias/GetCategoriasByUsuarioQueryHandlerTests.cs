using FluentAssertions;
using Moq;
using AutoMapper;
using SpendWise.Application.Queries.Categorias;
using SpendWise.Application.Handlers.Categorias;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using SpendWise.Domain.Enums;

namespace SpendWise.Application.Tests.Handlers.Categorias;

public class GetCategoriasByUsuarioQueryHandlerTests
{
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCategoriasByUsuarioQueryHandler _handler;

    public GetCategoriasByUsuarioQueryHandlerTests()
    {
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _unitOfWorkMock.Setup(x => x.Categorias).Returns(_categoriaRepositoryMock.Object);
        
        _handler = new GetCategoriasByUsuarioQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategorias_WhenUsuarioHasCategorias()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var categorias = new List<Categoria>
        {
            new("Alimentação", TipoCategoria.Despesa, usuarioId, "Categoria de alimentação", new Money(1000, "BRL")),
            new("Transporte", TipoCategoria.Despesa, usuarioId, "Categoria de transporte", new Money(500, "BRL")),
            new("Salário", TipoCategoria.Receita, usuarioId, "Categoria de salário")
        };

        var query = new GetCategoriasByUsuarioQuery(usuarioId);

        _categoriaRepositoryMock
            .Setup(x => x.GetByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(categorias);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<Application.DTOs.CategoriaDto>>(It.IsAny<IEnumerable<Categoria>>()))
            .Returns((IEnumerable<Categoria> source) => source.Select(c => new Application.DTOs.CategoriaDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao,
                Tipo = c.Tipo,
                UsuarioId = c.UsuarioId,
                IsAtiva = c.IsAtiva
            }));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Nome == "Alimentação");
        result.Should().Contain(c => c.Nome == "Transporte");
        result.Should().Contain(c => c.Nome == "Salário");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUsuarioHasNoCategorias()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var query = new GetCategoriasByUsuarioQuery(usuarioId);

        _categoriaRepositoryMock
            .Setup(x => x.GetByUsuarioIdAsync(usuarioId))
            .ReturnsAsync(new List<Categoria>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
