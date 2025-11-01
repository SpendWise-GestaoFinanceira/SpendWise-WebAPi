using MediatR;
using SpendWise.Application.Commands.OrcamentosMensais;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;
using SpendWise.Domain.Exceptions;
using FluentValidation;

namespace SpendWise.Application.Handlers.OrcamentosMensais;

public class UpdateOrcamentoMensalHandler : IRequestHandler<UpdateOrcamentoMensalCommand, OrcamentoMensalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateOrcamentoMensalCommand> _validator;

    public UpdateOrcamentoMensalHandler(
        IUnitOfWork unitOfWork,
        IValidator<UpdateOrcamentoMensalCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<OrcamentoMensalDto> Handle(UpdateOrcamentoMensalCommand request, CancellationToken cancellationToken)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Buscar orçamento existente
        var orcamentoMensal = await _unitOfWork.OrcamentosMensais.GetByIdAsync(request.Id);
        if (orcamentoMensal == null)
        {
            throw new ArgumentException($"Orçamento com ID {request.Id} não foi encontrado");
        }

        // Verificar se o mês está fechado
        var mesEstaFechado = await _unitOfWork.FechamentosMensais.MesEstaFechadoAsync(orcamentoMensal.UsuarioId, orcamentoMensal.AnoMes);
        if (mesEstaFechado)
        {
            throw new MesFechadoException(orcamentoMensal.AnoMes, "editar orçamentos");
        }

        // Verificar se o usuário tem permissão para atualizar este orçamento
        if (orcamentoMensal.UsuarioId != request.UsuarioId)
        {
            throw new UnauthorizedAccessException("Usuário não tem permissão para atualizar este orçamento");
        }

        // Criar Money object e atualizar valor do orçamento
        var valorMoney = new Money(request.Valor.Valor, request.Valor.Moeda);
        orcamentoMensal.AtualizarValor(valorMoney);

        // Salvar alterações usando o padrão correto
        _unitOfWork.OrcamentosMensais.Update(orcamentoMensal);

        // Criar período para buscar transações
        var periodo = CriarPeriodo(orcamentoMensal.AnoMes);
        
        // Calcular valores gastos para o DTO
        var valorGastoTotal = await _unitOfWork.Transacoes.GetTotalByTipoAsync(
            request.UsuarioId, 
            TipoTransacao.Despesa,
            periodo);

        var valorRestante = orcamentoMensal.Valor.Valor - valorGastoTotal;
        var percentualUtilizado = orcamentoMensal.Valor.Valor > 0 
            ? (valorGastoTotal / orcamentoMensal.Valor.Valor) * 100 
            : 0;

        // Converter para DTO
        return new OrcamentoMensalDto
        {
            Id = orcamentoMensal.Id,
            UsuarioId = orcamentoMensal.UsuarioId,
            AnoMes = orcamentoMensal.AnoMes,
            Valor = orcamentoMensal.Valor,
            CreatedAt = orcamentoMensal.CreatedAt,
            UpdatedAt = orcamentoMensal.UpdatedAt,
            ValorGasto = valorGastoTotal,
            ValorRestante = valorRestante,
            PercentualUtilizado = percentualUtilizado
        };
    }

    private static Periodo CriarPeriodo(string anoMes)
    {
        var parts = anoMes.Split('-');
        var ano = int.Parse(parts[0]);
        var mes = int.Parse(parts[1]);
        
        var inicio = new DateTime(ano, mes, 1);
        var fim = inicio.AddMonths(1).AddDays(-1);
        
        return new Periodo(inicio, fim);
    }
}
