using AutoMapper;
using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.Categorias;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.Categorias;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Categorias;

public class UpdateCategoriaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateCategoriaCommandHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _categoriaId = Guid.NewGuid();

    public UpdateCategoriaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock
            .Setup(u => u.Categorias)
            .Returns(_categoriaRepositoryMock.Object);

        _handler = new UpdateCategoriaCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAtualizarCategoria_QuandoExiste()
    {
        // Arrange
        var categoria = new Categoria("Nome Original", TipoCategoria.Despesa, _usuarioId);
        var categoriaDto = new CategoriaDto { Id = _categoriaId, Nome = "Nome Atualizado" };

        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync(categoria);

        _mapperMock
            .Setup(m => m.Map<CategoriaDto>(categoria))
            .Returns(categoriaDto);

        var command = new UpdateCategoriaCommand(
            _categoriaId,
            "Nome Atualizado",
            "Nova Descrição",
            "#FF5733",
            TipoCategoria.Despesa,
            true,
            1000m
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Nome Atualizado");
        _categoriaRepositoryMock.Verify(r => r.UpdateAsync(categoria), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarNull_QuandoNaoExiste()
    {
        // Arrange
        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync((Categoria?)null);

        var command = new UpdateCategoriaCommand(
            _categoriaId,
            "Nome",
            null,
            null,
            TipoCategoria.Despesa,
            true
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _categoriaRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Categoria>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveAtivarCategoria_QuandoIsAtivaTrue()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);
        categoria.Desativar(); // Desativa primeiro

        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync(categoria);

        _mapperMock
            .Setup(m => m.Map<CategoriaDto>(categoria))
            .Returns(new CategoriaDto());

        var command = new UpdateCategoriaCommand(
            _categoriaId,
            "Nome",
            null,
            null,
            TipoCategoria.Despesa,
            true // Ativar
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        categoria.IsAtiva.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DeveDesativarCategoria_QuandoIsAtivaFalse()
    {
        // Arrange
        var categoria = new Categoria("Nome", TipoCategoria.Despesa, _usuarioId);

        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync(categoria);

        _mapperMock
            .Setup(m => m.Map<CategoriaDto>(categoria))
            .Returns(new CategoriaDto());

        var command = new UpdateCategoriaCommand(
            _categoriaId,
            "Nome",
            null,
            null,
            TipoCategoria.Despesa,
            false // Desativar
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        categoria.IsAtiva.Should().BeFalse();
    }
}
