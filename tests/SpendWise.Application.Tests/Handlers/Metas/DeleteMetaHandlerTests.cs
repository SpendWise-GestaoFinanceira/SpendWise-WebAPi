using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.Metas;
using SpendWise.Application.Handlers;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Metas;

public class DeleteMetaHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMetaRepository> _metaRepositoryMock;
    private readonly DeleteMetaHandler _handler;
    private readonly Guid _metaId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();

    public DeleteMetaHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _metaRepositoryMock = new Mock<IMetaRepository>();

        _unitOfWorkMock.Setup(u => u.Metas).Returns(_metaRepositoryMock.Object);

        _handler = new DeleteMetaHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_DeveDeletarMeta_QuandoExiste()
    {
        // Arrange
        var meta = new Meta("Meta Teste", "Descrição", new Money(5000), DateTime.UtcNow.AddMonths(3), _usuarioId);

        _metaRepositoryMock
            .Setup(r => r.GetByIdAsync(_metaId))
            .ReturnsAsync(meta);

        var command = new DeleteMetaCommand(_metaId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _metaRepositoryMock.Verify(r => r.DeleteAsync(_metaId), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalse_QuandoNaoExiste()
    {
        // Arrange
        _metaRepositoryMock
            .Setup(r => r.GetByIdAsync(_metaId))
            .ReturnsAsync((Meta?)null);

        var command = new DeleteMetaCommand(_metaId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _metaRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
