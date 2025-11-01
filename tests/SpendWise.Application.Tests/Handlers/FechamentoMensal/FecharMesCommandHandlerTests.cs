using AutoMapper;
using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.FechamentoMensal;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.FechamentoMensal;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.FechamentoMensal;

public class FecharMesCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFechamentoMensalRepository> _fechamentoRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly FecharMesCommandHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public FecharMesCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fechamentoRepositoryMock = new Mock<IFechamentoMensalRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.FechamentosMensais).Returns(_fechamentoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Usuarios).Returns(_usuarioRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);

        _handler = new FecharMesCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_DeveFecharMes_ComSucesso()
    {
        // Arrange
        var usuario = new Usuario("Teste", new Email("teste@email.com"), "hash");
        var command = new FecharMesCommand(_usuarioId, "2025-10");

        var categoriaId = Guid.NewGuid();
        var transacoes = new List<Transacao>
        {
            new Transacao("Receita", new Money(3000), new DateTime(2025, 10, 5), TipoTransacao.Receita, _usuarioId, categoriaId),
            new Transacao("Despesa", new Money(1500), new DateTime(2025, 10, 10), TipoTransacao.Despesa, _usuarioId, categoriaId)
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(_usuarioId))
            .ReturnsAsync(usuario);

        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, "2025-10"))
            .ReturnsAsync((Domain.Entities.FechamentoMensal?)null);

        _transacaoRepositoryMock
            .Setup(r => r.GetByUsuarioIdAsync(_usuarioId))
            .ReturnsAsync(transacoes);

        _mapperMock
            .Setup(m => m.Map<FechamentoMensalDto>(It.IsAny<Domain.Entities.FechamentoMensal>()))
            .Returns(new FechamentoMensalDto { AnoMes = "2025-10" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AnoMes.Should().Be("2025-10");
        _fechamentoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.FechamentoMensal>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoUsuarioNaoExiste()
    {
        // Arrange
        var command = new FecharMesCommand(_usuarioId, "2025-10");

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(_usuarioId))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Usuário não encontrado*");
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoMesJaFechado()
    {
        // Arrange
        var usuario = new Usuario("Teste", new Email("teste@email.com"), "hash");
        var fechamentoExistente = new Domain.Entities.FechamentoMensal(_usuarioId, "2025-10", 3000, 1500, "Fechamento");
        var command = new FecharMesCommand(_usuarioId, "2025-10");

        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(_usuarioId))
            .ReturnsAsync(usuario);

        _fechamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(fechamentoExistente);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já está fechado*");
    }
}
