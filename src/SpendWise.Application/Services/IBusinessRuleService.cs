using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Services;

public interface IBusinessRuleService
{
    Task<ValidationResult> ValidarCriacaoTransacaoAsync(Guid usuarioId, TipoTransacao tipo, Guid? categoriaId, Money valor, DateTime data, CancellationToken cancellationToken = default);
    Task<StatusLimite> VerificarLimiteCategoriaAsync(Guid categoriaId, Money valorAdicional, CancellationToken cancellationToken = default);
    Task<bool> ValidarOrcamentoMensalAsync(Guid usuarioId, Money valorDespesa, DateTime data, CancellationToken cancellationToken = default);
    Task<bool> ValidarPrioridadeEssencialSuperfluoAsync(Guid usuarioId, Guid categoriaId, Money valor, DateTime data, CancellationToken cancellationToken = default);
    Task<decimal> CalcularGastoMensalCategoriaAsync(Guid categoriaId, DateTime data, CancellationToken cancellationToken = default);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
    
    public static ValidationResult WithWarning(params string[] warnings) => new()
    {
        IsValid = true,
        Warnings = warnings.ToList()
    };

    public void AddError(string message) => Errors.Add(message);
    public void AddWarning(string message) => Warnings.Add(message);
}
