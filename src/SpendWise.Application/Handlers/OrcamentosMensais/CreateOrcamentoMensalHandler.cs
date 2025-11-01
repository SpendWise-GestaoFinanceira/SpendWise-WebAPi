using MediatR;
using SpendWise.Application.Commands.OrcamentosMensais;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using SpendWise.Domain.Exceptions;
using FluentValidation;

namespace SpendWise.Application.Handlers.OrcamentosMensais;

public class CreateOrcamentoMensalHandler : IRequestHandler<CreateOrcamentoMensalCommand, OrcamentoMensalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateOrcamentoMensalCommand> _validator;

    public CreateOrcamentoMensalHandler(
        IUnitOfWork unitOfWork,
        IValidator<CreateOrcamentoMensalCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<OrcamentoMensalDto> Handle(CreateOrcamentoMensalCommand request, CancellationToken cancellationToken)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Verificar se usuário existe
        var usuario = await _unitOfWork.Usuarios.BuscarPorIdAsync(request.UsuarioId);
        if (usuario == null)
        {
            throw new ArgumentException($"Usuário com ID {request.UsuarioId} não foi encontrado");
        }

        // Verificar se o mês está fechado
        var mesEstaFechado = await _unitOfWork.FechamentosMensais.MesEstaFechadoAsync(request.UsuarioId, request.AnoMes);
        if (mesEstaFechado)
        {
            throw new MesFechadoException(request.AnoMes, "criar orçamentos");
        }

        // Verificar se já existe orçamento para este usuário e período
        var orcamentoExistente = await _unitOfWork.OrcamentosMensais.GetByUsuarioEAnoMesAsync(request.UsuarioId, request.AnoMes);
        if (orcamentoExistente != null)
        {
            throw new InvalidOperationException($"Já existe um orçamento para o usuário {request.UsuarioId} no período {request.AnoMes}");
        }

        // Criar Money object
        var valorMoney = new Money(request.Valor.Valor, request.Valor.Moeda);

        // Criar nova entidade
        var orcamentoMensal = new OrcamentoMensal(
            request.UsuarioId,
            request.AnoMes,
            valorMoney);

        // Salvar no repositório
        await _unitOfWork.OrcamentosMensais.AddAsync(orcamentoMensal);

        // Converter para DTO
        return new OrcamentoMensalDto
        {
            Id = orcamentoMensal.Id,
            UsuarioId = orcamentoMensal.UsuarioId,
            AnoMes = orcamentoMensal.AnoMes,
            Valor = orcamentoMensal.Valor,
            CreatedAt = orcamentoMensal.CreatedAt,
            UpdatedAt = orcamentoMensal.UpdatedAt,
            ValorGasto = 0, // Será calculado posteriormente com base nas transações
            ValorRestante = orcamentoMensal.Valor.Valor,
            PercentualUtilizado = 0
        };
    }
}
