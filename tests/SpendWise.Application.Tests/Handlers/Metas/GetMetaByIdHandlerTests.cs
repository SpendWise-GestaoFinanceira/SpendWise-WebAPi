using AutoMapper;
using FluentAssertions;
using Moq;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers;
using SpendWise.Application.Queries.Metas;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Metas;

public class GetMetaByIdHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMetaRepository> _metaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetMetaByIdHandler _handler;
    private readonly Guid _metaId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();

    public GetMetaByIdHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _metaRepositoryMock = new Mock<IMetaRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Metas).Returns(_metaRepositoryMock.Object);

        _handler = new GetMetaByIdHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarMeta_QuandoExiste()
    {
        // Arrange
        var meta = new Meta("Viagem", "Descrição", new Money(10000), DateTime.UtcNow.AddMonths(6), _usuarioId);
        var metaDto = new MetaDto { Id = _metaId, Descricao = "Descrição" };

        _metaRepositoryMock
            .Setup(r => r.GetByIdAsync(_metaId))
            .ReturnsAsync(meta);

        _mapperMock
            .Setup(m => m.Map<MetaDto>(meta))
            .Returns(metaDto);

        var query = new GetMetaByIdQuery(_metaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Descricao.Should().Be("Descrição");
        _metaRepositoryMock.Verify(r => r.GetByIdAsync(_metaId), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarNull_QuandoNaoExiste()
    {
        // Arrange
        _metaRepositoryMock
            .Setup(r => r.GetByIdAsync(_metaId))
            .ReturnsAsync((Meta?)null);

        var query = new GetMetaByIdQuery(_metaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _mapperMock.Verify(m => m.Map<MetaDto>(It.IsAny<Meta>()), Times.Never);
    }
}
