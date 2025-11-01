using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Validators.BusinessRules;

public interface IBusinessRule
{
    Task<BusinessRuleResult> ValidateAsync(BusinessRuleContext context);
}

public class BusinessRuleContext
{
    public Guid UsuarioId { get; set; }
    public TipoTransacao Tipo { get; set; }
    public Guid? CategoriaId { get; set; }
    public Money Valor { get; set; } = null!;
    public DateTime Data { get; set; }
}

public class BusinessRuleResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static BusinessRuleResult Success() => new() { IsValid = true };
    public static BusinessRuleResult Failure(string error) => new() { IsValid = false, Errors = { error } };
    public static BusinessRuleResult Warning(string warning) => new() { IsValid = true, Warnings = { warning } };
}
