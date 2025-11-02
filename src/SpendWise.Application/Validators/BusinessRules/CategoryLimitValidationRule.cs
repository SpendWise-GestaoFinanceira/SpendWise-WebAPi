using SpendWise.Application.Validators.BusinessRules;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Validators.BusinessRules;

public class CategoryLimitValidationRule : IBusinessRule
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryLimitValidationRule(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BusinessRuleResult> ValidateAsync(BusinessRuleContext context)
    {
        // Só aplica para despesas com categoria
        if (context.Tipo != TipoTransacao.Despesa || !context.CategoriaId.HasValue)
            return BusinessRuleResult.Success();

        var categoria = await _unitOfWork.Categorias.GetByIdAsync(context.CategoriaId.Value);
        if (categoria?.Limite is null)
            return BusinessRuleResult.Success();

        var gastoAtual = await CalcularGastoMensalAsync(context.CategoriaId.Value, context.Data);
        var gastoTotal = gastoAtual + context.Valor.Valor;
        var percentualUtilizado = gastoTotal / categoria.Limite.Valor * 100;

        return percentualUtilizado switch
        {
            > 100 => BusinessRuleResult.Failure("O valor da despesa excede o limite da categoria."),
            >= 80 => BusinessRuleResult.Warning("Atenção: Esta despesa aproxima você do limite da categoria."),
            _ => BusinessRuleResult.Success()
        };
    }

    private async Task<decimal> CalcularGastoMensalAsync(Guid categoriaId, DateTime data)
    {
        var periodo = new Periodo(
            new DateTime(data.Year, data.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month), 23, 59, 59, DateTimeKind.Utc));

        var transacoes = await _unitOfWork.Transacoes.GetByCategoriaAsync(categoriaId);

        return transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa &&
                       t.DataTransacao >= periodo.DataInicio &&
                       t.DataTransacao <= periodo.DataFim)
            .Sum(t => t.Valor.Valor);
    }
}
