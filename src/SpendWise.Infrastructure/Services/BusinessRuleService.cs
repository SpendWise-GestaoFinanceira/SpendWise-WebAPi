using Microsoft.Extensions.Logging;
using SpendWise.Application.Services;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Infrastructure.Services;

public class BusinessRuleService : IBusinessRuleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BusinessRuleService> _logger;

    public BusinessRuleService(IUnitOfWork unitOfWork, ILogger<BusinessRuleService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidarCriacaoTransacaoAsync(
        Guid usuarioId,
        TipoTransacao tipo,
        Guid? categoriaId,
        Money valor,
        DateTime data,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validando criação de transação para usuário {UsuarioId}", usuarioId);

        var errors = new List<string>();
        var warnings = new List<string>();

        // 1. Validação temporal - data não pode ser futura
        if (data.Date > DateTime.Now.Date)
        {
            errors.Add("A data da transação não pode ser futura");
        }

        // 2. Para despesas, categoria é obrigatória
        if (tipo == TipoTransacao.Despesa && categoriaId == null)
        {
            errors.Add("Categoria é obrigatória para despesas");
        }

        // 3. Para receitas, categoria não deve ser informada
        if (tipo == TipoTransacao.Receita && categoriaId.HasValue)
        {
            warnings.Add("Categoria será ignorada para receitas");
        }

        // Para despesas, validar regras específicas
        if (tipo == TipoTransacao.Despesa && categoriaId.HasValue)
        {
            // 4. Validar limite da categoria
            var statusLimite = await VerificarLimiteCategoriaAsync(categoriaId.Value, valor, cancellationToken);
            if (statusLimite == StatusLimite.Excedido)
            {
                errors.Add("Esta despesa faria com que o limite da categoria fosse ultrapassado");
            }
            else if (statusLimite == StatusLimite.Alerta)
            {
                warnings.Add("Atenção: Esta despesa fará com que a categoria atinja mais de 80% do limite");
            }

            // 5. Validar orçamento mensal
            var orcamentoValido = await ValidarOrcamentoMensalAsync(usuarioId, valor, data, cancellationToken);
            if (!orcamentoValido)
            {
                errors.Add("Esta despesa faria com que o orçamento mensal fosse ultrapassado");
            }

            // 6. Validar prioridade essencial vs supérfluo
            var prioridadeValida = await ValidarPrioridadeEssencialSuperfluoAsync(usuarioId, categoriaId.Value, valor, data, cancellationToken);
            if (!prioridadeValida)
            {
                errors.Add("Não é possível adicionar despesas supérfluas quando categorias essenciais estão comprometidas");
            }
        }

        return errors.Any()
            ? ValidationResult.Failure(errors.ToArray())
            : warnings.Any()
                ? ValidationResult.WithWarning(warnings.ToArray())
                : ValidationResult.Success();
    }

    public async Task<StatusLimite> VerificarLimiteCategoriaAsync(
        Guid categoriaId,
        Money valorAdicional,
        CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(categoriaId);
        if (categoria == null)
            return StatusLimite.SemLimite;

        var gastoAtual = await CalcularGastoMensalCategoriaAsync(categoriaId, DateTime.Now, cancellationToken);
        var novoGasto = gastoAtual + valorAdicional.Valor;

        return categoria.VerificarStatusLimite(novoGasto);
    }

    public async Task<bool> ValidarOrcamentoMensalAsync(
        Guid usuarioId,
        Money valorDespesa,
        DateTime data,
        CancellationToken cancellationToken = default)
    {
        var anoMes = $"{data.Year:0000}-{data.Month:00}";
        var orcamento = await _unitOfWork.OrcamentosMensais.GetByUsuarioEAnoMesAsync(usuarioId, anoMes);

        if (orcamento == null)
            return true; // Sem orçamento definido, permite

        // Calcular total de despesas do mês
        var transacoesMes = await _unitOfWork.Transacoes.BuscarPorPeriodoComCategoriasAsync(
            usuarioId,
            new DateTime(data.Year, data.Month, 1),
            new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month)),
            null,
            cancellationToken);

        var totalDespesasMes = transacoesMes
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor.Valor);

        var novoTotal = totalDespesasMes + valorDespesa.Valor;

        return novoTotal <= orcamento.Valor.Valor;
    }

    public async Task<bool> ValidarPrioridadeEssencialSuperfluoAsync(
        Guid usuarioId,
        Guid categoriaId,
        Money valor,
        DateTime data,
        CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(categoriaId);
        if (categoria == null)
            return false;

        // Se a categoria é essencial, sempre pode adicionar
        if (categoria.Prioridade == PrioridadeCategoria.Essencial)
            return true;

        // Se é supérflua, verificar se essenciais estão controladas
        var categoriasEssenciais = await _unitOfWork.Categorias.GetByUsuarioIdAsync(usuarioId);
        var essenciais = categoriasEssenciais.Where(c => c.Prioridade == PrioridadeCategoria.Essencial);

        foreach (var essencial in essenciais)
        {
            var gastoEssencial = await CalcularGastoMensalCategoriaAsync(essencial.Id, data, cancellationToken);
            var statusEssencial = essencial.VerificarStatusLimite(gastoEssencial);

            // Se alguma categoria essencial está em alerta ou excedida, bloquear supérfluas
            if (statusEssencial == StatusLimite.Alerta || statusEssencial == StatusLimite.Excedido)
            {
                _logger.LogWarning("Bloqueando despesa supérflua pois categoria essencial {CategoriaEssencial} está comprometida",
                    essencial.Nome);
                return false;
            }
        }

        return true;
    }

    public async Task<decimal> CalcularGastoMensalCategoriaAsync(
        Guid categoriaId,
        DateTime data,
        CancellationToken cancellationToken = default)
    {
        var inicioMes = new DateTime(data.Year, data.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var transacoes = await _unitOfWork.Transacoes.BuscarPorPeriodoComCategoriasAsync(
            Guid.Empty, // Será filtrado pela categoria
            inicioMes,
            fimMes,
            new List<Guid> { categoriaId },
            cancellationToken);

        return transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor.Valor);
    }
}
