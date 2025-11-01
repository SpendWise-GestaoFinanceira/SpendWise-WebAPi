using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using SpendWise.Application.Commands.OrcamentosMensais;
using SpendWise.Application.Handlers.OrcamentosMensais;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.OrcamentosMensais;

public class CreateOrcamentoMensalHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrcamentoMensalRepository> _orcamentoRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IFechamentoMensalRepository> _fechamentoRepositoryMock;
    private readonly Mock<IValidator<CreateOrcamentoMensalCommand>> _validatorMock;
    private readonly CreateOrcamentoMensalHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public CreateOrcamentoMensalHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orcamentoRepositoryMock = new Mock<IOrcamentoMensalRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _fechamentoRepositoryMock = new Mock<IFechamentoMensalRepository>();
        _validatorMock = new Mock<IValidator<CreateOrcamentoMensalCommand>>();

        _unitOfWorkMock.Setup(u => u.OrcamentosMensais).Returns(_orcamentoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Usuarios).Returns(_usuarioRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FechamentosMensais).Returns(_fechamentoRepositoryMock.Object);

        // Validator retorna sucesso por padrão
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateOrcamentoMensalCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = new CreateOrcamentoMensalHandler(_unitOfWorkMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task Handle_DeveCriarOrcamento_ComSucesso()
    {
        // Arrange
        var usuario = new Usuario("Teste", new Email("teste@email.com"), "hash");
        var command = new CreateOrcamentoMensalCommand(
            _usuarioId,
            "2025-10",
            new Money(5000)
        );

        _usuarioRepositoryMock
            .Setup(r => r.BuscarPorIdAsync(_usuarioId))
            .ReturnsAsync(usuario);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(false);

        _orcamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, "2025-10"))
            .ReturnsAsync((OrcamentoMensal?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UsuarioId.Should().Be(_usuarioId);
        result.AnoMes.Should().Be("2025-10");
        result.Valor.Valor.Should().Be(5000);
        _orcamentoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OrcamentoMensal>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoUsuarioNaoExiste()
    {
        // Arrange
        var command = new CreateOrcamentoMensalCommand(
            _usuarioId,
            "2025-10",
            new Money(5000)
        );

        _usuarioRepositoryMock
            .Setup(r => r.BuscarPorIdAsync(_usuarioId))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"*{_usuarioId}*");
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoOrcamentoJaExiste()
    {
        // Arrange
        var usuario = new Usuario("Teste", new Email("teste@email.com"), "hash");
        var orcamentoExistente = new OrcamentoMensal(_usuarioId, "2025-10", new Money(3000));
        
        var command = new CreateOrcamentoMensalCommand(
            _usuarioId,
            "2025-10",
            new Money(5000)
        );

        _usuarioRepositoryMock
            .Setup(r => r.BuscarPorIdAsync(_usuarioId))
            .ReturnsAsync(usuario);

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(false);

        _orcamentoRepositoryMock
            .Setup(r => r.GetByUsuarioEAnoMesAsync(_usuarioId, "2025-10"))
            .ReturnsAsync(orcamentoExistente);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Já existe um orçamento*");
    }
}
