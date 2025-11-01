using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Application.Validators.BusinessRules;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Exceptions;
using SpendWise.Domain.Utils;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.Transacoes;

public class CreateTransacaoCommandHandler : IRequestHandler<CreateTransacaoCommand, TransacaoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEnumerable<IBusinessRule> _businessRules;

    public CreateTransacaoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IEnumerable<IBusinessRule> businessRules)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<TransacaoDto> Handle(CreateTransacaoCommand request, CancellationToken cancellationToken)
    {
        // Verificar se o mês está fechado
        var anoMes = DateUtils.ToAnoMesString(request.DataTransacao);
        var mesEstaFechado = await _unitOfWork.FechamentosMensais.MesEstaFechadoAsync(request.UsuarioId, anoMes);
        
        if (mesEstaFechado)
        {
            throw new MesFechadoException(anoMes, "criar transações");
        }

        // Validar regras de negócio
        var valor = new Money(request.Valor);
        var context = new BusinessRuleContext
        {
            UsuarioId = request.UsuarioId,
            Tipo = request.Tipo,
            CategoriaId = request.CategoriaId,
            Valor = valor,
            Data = request.DataTransacao
        };

        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var rule in _businessRules)
        {
            var ruleResult = await rule.ValidateAsync(context);
            if (!ruleResult.IsValid)
            {
                errors.AddRange(ruleResult.Errors);
            }
            warnings.AddRange(ruleResult.Warnings);
        }

        if (errors.Any())
        {
            var errorMessage = string.Join("; ", errors);
            throw new BusinessRuleViolationException(errorMessage);
        }

        var transacao = new Transacao(
            request.Descricao,
            request.Valor,
            request.DataTransacao,
            request.Tipo,
            request.UsuarioId,
            request.CategoriaId,
            request.Observacoes
        );

        var result = await _unitOfWork.Transacoes.AddAsync(transacao);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TransacaoDto>(result);
    }
}