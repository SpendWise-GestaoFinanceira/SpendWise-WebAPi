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

public class GetCategoriaByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCategoriaByIdQueryHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _categoriaId = Guid.NewGuid();

    public GetCategoriaByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock
            .Setup(u => u.Categorias)
            .Returns(_categoriaRepositoryMock.Object);

        _handler = new GetCategoriaByIdQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarCategoria_QuandoExiste()
    {
        // Arrange
        var categoria = new Categoria("Alimentação", TipoCategoria.Despesa, _usuarioId);
        var categoriaDto = new CategoriaDto { Id = _categoriaId, Nome = "Alimentação" };

        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync(categoria);

        _mapperMock
            .Setup(m => m.Map<CategoriaDto>(categoria))
            .Returns(categoriaDto);

        var query = new GetCategoriaByIdQuery(_categoriaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Alimentação");
        _categoriaRepositoryMock.Verify(r => r.GetByIdAsync(_categoriaId), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarNull_QuandoNaoExiste()
    {
        // Arrange
        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync((Categoria?)null);

        var query = new GetCategoriaByIdQuery(_categoriaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _mapperMock.Verify(m => m.Map<CategoriaDto>(It.IsAny<Categoria>()), Times.Never);
    }

    [Theory]
    [InlineData("Alimentação")]
    [InlineData("Transporte")]
    [InlineData("Saúde")]
    public async Task Handle_DeveFuncionarComDiferentesNomes(string nome)
    {
        // Arrange
        var categoria = new Categoria(nome, TipoCategoria.Despesa, _usuarioId);
        var categoriaDto = new CategoriaDto { Id = _categoriaId, Nome = nome };

        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync(categoria);

        _mapperMock
            .Setup(m => m.Map<CategoriaDto>(categoria))
            .Returns(categoriaDto);

        var query = new GetCategoriaByIdQuery(_categoriaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.Nome.Should().Be(nome);
    }
}
