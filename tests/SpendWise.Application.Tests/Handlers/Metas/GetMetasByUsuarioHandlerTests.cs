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

public class GetMetasByUsuarioHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMetaRepository> _metaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetMetasByUsuarioHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public GetMetasByUsuarioHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _metaRepositoryMock = new Mock<IMetaRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Metas).Returns(_metaRepositoryMock.Object);

        _handler = new GetMetasByUsuarioHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarTodasMetas_QuandoApenasAtivasFalse()
    {
        // Arrange
        var metas = new List<Meta>
        {
            new Meta("Meta 1", "Desc 1", new Money(5000), DateTime.UtcNow.AddMonths(3), _usuarioId),
            new Meta("Meta 2", "Desc 2", new Money(10000), DateTime.UtcNow.AddMonths(6), _usuarioId)
        };

        var metasDto = new List<MetaResumoDto>
        {
            new MetaResumoDto { Descricao = "Desc 1" },
            new MetaResumoDto { Descricao = "Desc 2" }
        };

        _metaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(metas);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<MetaResumoDto>>(metas))
            .Returns(metasDto);

        var query = new GetMetasByUsuarioQuery(_usuarioId, false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        _metaRepositoryMock.Verify(r => r.GetByUsuarioIdAsync(_usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarApenasAtivas_QuandoApenasAtivasTrue()
    {
        // Arrange
        var metas = new List<Meta>
        {
            new Meta("Meta Ativa", "Desc", new Money(5000), DateTime.UtcNow.AddMonths(3), _usuarioId)
        };

        var metasDto = new List<MetaResumoDto>
        {
            new MetaResumoDto { Descricao = "Desc" }
        };

        _metaRepositoryMock
            .Setup(r => r.GetAtivasByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(metas);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<MetaResumoDto>>(metas))
            .Returns(metasDto);

        var query = new GetMetasByUsuarioQuery(_usuarioId, true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        _metaRepositoryMock.Verify(r => r.GetAtivasByUsuarioIdAsync(_usuarioId), Times.Once);
        _metaRepositoryMock.Verify(r => r.GetByUsuarioIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveRetornarListaVazia_QuandoNaoHaMetas()
    {
        // Arrange
        _metaRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(new List<Meta>());

        _mapperMock
            .Setup(m => m.Map<IEnumerable<MetaResumoDto>>(It.IsAny<List<Meta>>()))
            .Returns(new List<MetaResumoDto>());

        var query = new GetMetasByUsuarioQuery(_usuarioId, false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
