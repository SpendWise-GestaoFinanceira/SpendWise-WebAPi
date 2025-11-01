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

public class CreateMetaHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMetaRepository> _metaRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBusinessRule> _businessRuleMock;
    private readonly CreateMetaHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public CreateMetaHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _metaRepositoryMock = new Mock<IMetaRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _mapperMock = new Mock<IMapper>();
        _businessRuleMock = new Mock<IBusinessRule>();

        _unitOfWorkMock.Setup(u => u.Metas).Returns(_metaRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Usuarios).Returns(_usuarioRepositoryMock.Object);

        var businessRules = new List<IBusinessRule> { _businessRuleMock.Object };
        _handler = new CreateMetaHandler(_unitOfWorkMock.Object, _mapperMock.Object, businessRules);
    }

    [Fact]
    public async Task Handle_DeveCriarMeta_ComSucesso()
    {
        // Arrange
        var usuario = new Usuario("Teste", new Email("teste@email.com"), "hash");
        var prazo = DateTime.UtcNow.AddMonths(6);
        var command = new CreateMetaCommand(
            "Viagem Europa",
            "Economizar para viagem",
            10000m,
            prazo,
            _usuarioId,
            0
        );

        var metaCriada = new Meta("Viagem Europa", "Economizar para viagem", new Money(10000), prazo, _usuarioId);
        var metaDto = new MetaDto { Id = Guid.NewGuid(), Descricao = "Economizar para viagem" };

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(_usuarioId))
            .ReturnsAsync(usuario);

        _metaRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Meta>()))
            .ReturnsAsync(metaCriada);

        _mapperMock
            .Setup(m => m.Map<MetaDto>(It.IsAny<Meta>()))
            .Returns(metaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Descricao.Should().Be("Economizar para viagem");
        _metaRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Meta>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoUsuarioNaoExiste()
    {
        // Arrange
        var command = new CreateMetaCommand(
            "Meta Teste",
            "Descrição",
            5000m,
            DateTime.UtcNow.AddMonths(3),
            _usuarioId
        );

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(_usuarioId))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Usuário não encontrado*");
    }

    [Fact]
    public async Task Handle_DeveAdicionarValorInicial_QuandoFornecido()
    {
        // Arrange
        var usuario = new Usuario("Teste", new Email("teste@email.com"), "hash");
        var prazo = DateTime.UtcNow.AddMonths(6);
        var command = new CreateMetaCommand(
            "Meta com Valor Inicial",
            "Descrição",
            10000m,
            prazo,
            _usuarioId,
            2000m // Valor inicial
        );

        var metaCriada = new Meta("Meta com Valor Inicial", "Descrição", new Money(10000), prazo, _usuarioId);
        var metaDto = new MetaDto { Id = Guid.NewGuid(), Descricao = "Descrição" };

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(_usuarioId))
            .ReturnsAsync(usuario);

        _metaRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Meta>()))
            .ReturnsAsync(metaCriada);

        _mapperMock
            .Setup(m => m.Map<MetaDto>(It.IsAny<Meta>()))
            .Returns(metaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _metaRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Meta>()), Times.Once);
    }
}
