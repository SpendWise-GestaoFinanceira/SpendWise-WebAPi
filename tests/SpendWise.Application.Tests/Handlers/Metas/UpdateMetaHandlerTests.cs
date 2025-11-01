using AutoMapper;
using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.Metas;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers;
using SpendWise.Application.Validators.BusinessRules;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Metas;

public class UpdateMetaHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMetaRepository> _metaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBusinessRule> _businessRuleMock;
    private readonly UpdateMetaHandler _handler;
    private readonly Guid _metaId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();

    public UpdateMetaHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _metaRepositoryMock = new Mock<IMetaRepository>();
        _mapperMock = new Mock<IMapper>();
        _businessRuleMock = new Mock<IBusinessRule>();

        _unitOfWorkMock.Setup(u => u.Metas).Returns(_metaRepositoryMock.Object);

        var businessRules = new List<IBusinessRule> { _businessRuleMock.Object };
        _handler = new UpdateMetaHandler(_unitOfWorkMock.Object, _mapperMock.Object, businessRules);
    }

    [Fact]
    public async Task Handle_DeveAtualizarMeta_ComSucesso()
    {
        // Arrange
        var meta = new Meta("Nome Original", "Desc", new Money(10000), DateTime.UtcNow.AddMonths(6), _usuarioId);
        var metaDto = new MetaDto { Id = _metaId, Descricao = "Nova Descrição" };

        _metaRepositoryMock
            .Setup(r => r.GetByIdAsync(_metaId))
            .ReturnsAsync(meta);

        _mapperMock
            .Setup(m => m.Map<MetaDto>(meta))
            .Returns(metaDto);

        var command = new UpdateMetaCommand(
            _metaId,
            "Novo Nome",
            "Nova Descrição",
            15000m,
            DateTime.UtcNow.AddMonths(12)
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _metaRepositoryMock.Verify(r => r.UpdateAsync(meta), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoMetaNaoExiste()
    {
        // Arrange
        _metaRepositoryMock
            .Setup(r => r.GetByIdAsync(_metaId))
            .ReturnsAsync((Meta?)null);

        var command = new UpdateMetaCommand(_metaId, "Nome");

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Meta não encontrada*");
    }

    [Fact]
    public async Task Handle_DeveAtualizarApenasNome_QuandoSomenteNomeFornecido()
    {
        // Arrange
        var meta = new Meta("Nome Original", "Desc", new Money(10000), DateTime.UtcNow.AddMonths(6), _usuarioId);
        var metaDto = new MetaDto { Id = _metaId };

        _metaRepositoryMock
            .Setup(r => r.GetByIdAsync(_metaId))
            .ReturnsAsync(meta);

        _mapperMock
            .Setup(m => m.Map<MetaDto>(meta))
            .Returns(metaDto);

        var command = new UpdateMetaCommand(_metaId, "Novo Nome");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _metaRepositoryMock.Verify(r => r.UpdateAsync(meta), Times.Once);
    }
}
