using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Services;

public interface IBusinessRuleService
{
    Task<BusinessValidationResult> ValidarCriacaoTransacao(Guid usuarioId, Guid? categoriaId, Money valor, DateTime dataTransacao);
    Task<StatusLimite> VerificarLimiteCategoria(Guid categoriaId, Money valor, DateTime dataTransacao);
    Task<bool> ValidarOrcamentoMensal(Guid usuarioId, Money valor, DateTime dataTransacao);
    Task<bool> ValidarPrioridadeEssencialSuperfluo(Guid usuarioId, Guid categoriaId, Money valor, DateTime dataTransacao);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public void AddError(string message) => Errors.Add(message);
    public void AddWarning(string message) => Warnings.Add(message);
}
