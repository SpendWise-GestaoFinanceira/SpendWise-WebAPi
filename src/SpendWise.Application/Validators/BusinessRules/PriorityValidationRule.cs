using SpendWise.Application.Validators.BusinessRules;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Validators.BusinessRules;

public class PriorityValidationRule : IBusinessRule
{
    private readonly IUnitOfWork _unitOfWork;

    public PriorityValidationRule(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BusinessRuleResult> ValidateAsync(BusinessRuleContext context)
    {
        // Só aplica para despesas com categoria
        if (context.Tipo != TipoTransacao.Despesa || !context.CategoriaId.HasValue)
            return BusinessRuleResult.Success();

        var categoria = await _unitOfWork.Categorias.GetByIdAsync(context.CategoriaId.Value);
        if (categoria is null)
            return BusinessRuleResult.Failure("Categoria não encontrada.");

        // Se a categoria é essencial, sempre pode adicionar
        if (categoria.Prioridade == PrioridadeCategoria.Essencial)
            return BusinessRuleResult.Success();

        // Se é supérflua, verificar se categorias essenciais estão comprometidas
        var categoriasEssenciais = await _unitOfWork.Categorias.GetByUsuarioIdAsync(context.UsuarioId);
        var essenciais = categoriasEssenciais.Where(c => c.Prioridade == PrioridadeCategoria.Essencial && c.Limite is not null);

        foreach (var essencial in essenciais)
        {
            var gastoEssencial = await CalcularGastoMensalAsync(essencial.Id, context.Data);
            var statusEssencial = essencial.VerificarStatusLimite(gastoEssencial);

            // Se alguma categoria essencial está em alerta ou excedida, bloquear supérfluas
            if (statusEssencial == StatusLimite.Alerta || statusEssencial == StatusLimite.Excedido)
            {
                return BusinessRuleResult.Failure(
                    $"Despesa supérflua bloqueada: categoria essencial '{essencial.Nome}' está comprometida ({statusEssencial}). " +
                    "Quite primeiro os gastos essenciais antes de adicionar supérfluos.");
            }
        }

        return BusinessRuleResult.Success();
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
