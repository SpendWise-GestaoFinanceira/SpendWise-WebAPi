using SpendWise.Application.Validators.BusinessRules;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Validators.BusinessRules;

public class BudgetValidationRule : IBusinessRule
{
    private readonly IUnitOfWork _unitOfWork;

    public BudgetValidationRule(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BusinessRuleResult> ValidateAsync(BusinessRuleContext context)
    {
        // Só aplica para despesas
        if (context.Tipo != TipoTransacao.Despesa)
            return BusinessRuleResult.Success();

        var anoMes = $"{context.Data.Year:D4}-{context.Data.Month:D2}";
        var orcamento = await _unitOfWork.OrcamentosMensais.GetByUsuarioEAnoMesAsync(context.UsuarioId, anoMes);

        if (orcamento == null)
            return BusinessRuleResult.Success(); // Se não há orçamento definido, permite a despesa

        var periodo = new Periodo(
            new DateTime(context.Data.Year, context.Data.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(context.Data.Year, context.Data.Month, DateTime.DaysInMonth(context.Data.Year, context.Data.Month), 23, 59, 59, DateTimeKind.Utc));

        var totalDespesas = await _unitOfWork.Transacoes.GetTotalByTipoAsync(context.UsuarioId, TipoTransacao.Despesa, periodo);

        if ((totalDespesas + context.Valor.Valor) > orcamento.Valor.Valor)
        {
            return BusinessRuleResult.Failure("O valor da despesa excede o orçamento mensal disponível.");
        }

        return BusinessRuleResult.Success();
    }
}
