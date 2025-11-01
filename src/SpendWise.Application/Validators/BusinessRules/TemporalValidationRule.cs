using SpendWise.Application.Validators.BusinessRules;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Validators.BusinessRules;

public class TemporalValidationRule : IBusinessRule
{
    public Task<BusinessRuleResult> ValidateAsync(BusinessRuleContext context)
    {
        if (context.Data.Date > DateTime.Now.Date)
        {
            return Task.FromResult(BusinessRuleResult.Failure("Não é possível criar transações para datas futuras."));
        }

        return Task.FromResult(BusinessRuleResult.Success());
    }
}
