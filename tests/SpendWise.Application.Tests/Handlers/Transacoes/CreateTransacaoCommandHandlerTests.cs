using AutoMapper;
using FluentAssertions;
using Moq;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Application.DTOs;
using SpendWise.Application.Handlers.Transacoes;
using SpendWise.Application.Validators.BusinessRules;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Exceptions;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using Xunit;

namespace SpendWise.Application.Tests.Handlers.Transacoes;

public class CreateTransacaoCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITransacaoRepository> _transacaoRepositoryMock;
    private readonly Mock<IFechamentoMensalRepository> _fechamentoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBusinessRule> _businessRuleMock;
    private readonly CreateTransacaoCommandHandler _handler;
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _categoriaId = Guid.NewGuid();

    public CreateTransacaoCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transacaoRepositoryMock = new Mock<ITransacaoRepository>();
        _fechamentoRepositoryMock = new Mock<IFechamentoMensalRepository>();
        _mapperMock = new Mock<IMapper>();
        _businessRuleMock = new Mock<IBusinessRule>();

        _unitOfWorkMock.Setup(u => u.Transacoes).Returns(_transacaoRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FechamentosMensais).Returns(_fechamentoRepositoryMock.Object);

        _businessRuleMock
            .Setup(r => r.ValidateAsync(It.IsAny<BusinessRuleContext>()))
            .ReturnsAsync(new BusinessRuleResult { IsValid = true });

        var businessRules = new List<IBusinessRule> { _businessRuleMock.Object };
        _handler = new CreateTransacaoCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object, businessRules);
    }

    [Fact]
    public async Task Handle_DeveCriarTransacao_ComSucesso()
    {
        // Arrange
        var command = new CreateTransacaoCommand(
            "Compra Supermercado",
            new Money(150),
            DateTime.Now,
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId,
            "Compras do mês"
        );

        var transacao = new Transacao(
            command.Descricao,
            command.Valor,
            command.DataTransacao,
            command.Tipo,
            command.UsuarioId,
            command.CategoriaId,
            command.Observacoes
        );

        var transacaoDto = new TransacaoDto { Id = Guid.NewGuid(), Descricao = "Compra Supermercado" };

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, It.IsAny<string>()))
            .ReturnsAsync(false);

        _transacaoRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transacao>()))
            .ReturnsAsync(transacao);

        _mapperMock
            .Setup(m => m.Map<TransacaoDto>(It.IsAny<Transacao>()))
            .Returns(transacaoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Descricao.Should().Be("Compra Supermercado");
        _transacaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoMesEstaFechado()
    {
        // Arrange
        var command = new CreateTransacaoCommand(
            "Compra",
            new Money(100),
            DateTime.Now,
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId,
            null
        );

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<MesFechadoException>();
    }

    [Fact]
    public async Task Handle_DeveLancarException_QuandoRegraDeNegocioViolada()
    {
        // Arrange
        var command = new CreateTransacaoCommand(
            "Compra",
            new Money(100),
            DateTime.Now,
            TipoTransacao.Despesa,
            _usuarioId,
            _categoriaId,
            null
        );

        _fechamentoRepositoryMock
            .Setup(r => r.MesEstaFechadoAsync(_usuarioId, It.IsAny<string>()))
            .ReturnsAsync(false);

        _businessRuleMock
            .Setup(r => r.ValidateAsync(It.IsAny<BusinessRuleContext>()))
            .ReturnsAsync(new BusinessRuleResult 
            { 
                IsValid = false, 
                Errors = new List<string> { "Limite excedido" } 
            });

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }
}
