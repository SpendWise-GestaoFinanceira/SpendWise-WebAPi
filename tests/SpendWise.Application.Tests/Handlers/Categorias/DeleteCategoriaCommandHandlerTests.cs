using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.Categorias;
using SpendWise.Application.Handlers.Categorias;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Categorias;

public class DeleteCategoriaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly DeleteCategoriaCommandHandler _handler;
    private readonly Guid _categoriaId;
    private readonly Guid _usuarioId;

    public DeleteCategoriaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();

        _unitOfWorkMock.Setup(u => u.Categorias).Returns(_categoriaRepositoryMock.Object);

        _handler = new DeleteCategoriaCommandHandler(_unitOfWorkMock.Object);
        _categoriaId = Guid.NewGuid();
        _usuarioId = Guid.NewGuid();
    }

    [Fact]
    public async Task Handle_DeveDeletarCategoriaComSucesso()
    {
        // Arrange
        var categoria = new Categoria("Alimentação", TipoCategoria.Despesa, _usuarioId, "Gastos com comida");

        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync(categoria);

        var command = new DeleteCategoriaCommand(_categoriaId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _categoriaRepositoryMock.Verify(r => r.DeleteAsync(_categoriaId), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalse_QuandoCategoriaNaoExiste()
    {
        // Arrange
        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync((Categoria?)null);

        var command = new DeleteCategoriaCommand(_categoriaId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _categoriaRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NaoDeveCommitar_QuandoDeletaFalha()
    {
        // Arrange
        _categoriaRepositoryMock
            .Setup(r => r.GetByIdAsync(_categoriaId))
            .ReturnsAsync((Categoria?)null);

        var command = new DeleteCategoriaCommand(_categoriaId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
