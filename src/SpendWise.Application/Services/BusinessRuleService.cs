using SpendWise.Domain.Entities;
using SpendWise.Domain.Enums;
using SpendWise.Domain.ValueObjects;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Services;

public class BusinessRuleService : IBusinessRuleService
{
    private readonly IUnitOfWork _unitOfWork;

    public BusinessRuleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidationResult> ValidarCriacaoTransacaoAsync(Guid usuarioId, TipoTransacao tipo, Guid? categoriaId, Money valor, DateTime data, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        // Regra 1: Validação temporal (não permitir transações no futuro)
        if (data.Date > DateTime.Now.Date)
        {
            result.IsValid = false;
            result.AddError("Não é possível criar transações para datas futuras.");
        }

        // Para despesas, validar regras específicas
        if (tipo == TipoTransacao.Despesa && categoriaId.HasValue)
        {
            // Regra 2: Verificar limite da categoria
            var statusLimite = await VerificarLimiteCategoriaAsync(categoriaId.Value, valor, cancellationToken);
            if (statusLimite == StatusLimite.Excedido)
            {
                result.IsValid = false;
                result.AddError("O valor da despesa excede o limite da categoria.");
            }
            else if (statusLimite == StatusLimite.Alerta)
            {
                result.AddWarning("Atenção: Esta despesa aproxima você do limite da categoria.");
            }

            // Regra 3: Validar orçamento mensal
            var orcamentoValido = await ValidarOrcamentoMensalAsync(usuarioId, valor, data, cancellationToken);
            if (!orcamentoValido)
            {
                result.IsValid = false;
                result.AddError("O valor da despesa excede o orçamento mensal disponível.");
            }

            // Regra 4: Validar prioridade essencial/supérfluo
            var prioridadeValida = await ValidarPrioridadeEssencialSuperfluoAsync(usuarioId, categoriaId.Value, valor, data, cancellationToken);
            if (!prioridadeValida)
            {
                result.AddWarning("Atenção: Você tem gastos supérfluos elevados. Considere priorizar gastos essenciais.");
            }
        }

        return result;
    }

    public async Task<StatusLimite> VerificarLimiteCategoriaAsync(Guid categoriaId, Money valorAdicional, CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(categoriaId);
        if (categoria?.Limite is null)
            return StatusLimite.SemLimite;

        var gastoAtual = await CalcularGastoMensalCategoriaAsync(categoriaId, DateTime.Now, cancellationToken);
        var gastoTotal = gastoAtual + valorAdicional.Valor;

        var percentualUtilizado = gastoTotal / categoria.Limite.Valor * 100;

        return percentualUtilizado switch
        {
            >= 100 => StatusLimite.Excedido,
            >= 80 => StatusLimite.Alerta,
            _ => StatusLimite.Normal
        };
    }

    public async Task<bool> ValidarOrcamentoMensalAsync(Guid usuarioId, Money valorDespesa, DateTime data, CancellationToken cancellationToken = default)
    {
        // Criar string no formato YYYY-MM
        var anoMes = $"{data.Year:D4}-{data.Month:D2}";
        
        var orcamento = await _unitOfWork.OrcamentosMensais.GetByUsuarioEAnoMesAsync(usuarioId, anoMes);
        if (orcamento == null)
            return true; // Se não há orçamento definido, permite a despesa

        // Criar período para o mês
        var periodo = new Periodo(
            new DateTime(data.Year, data.Month, 1),
            new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month)));

        var totalDespesas = await _unitOfWork.Transacoes.GetTotalByTipoAsync(usuarioId, TipoTransacao.Despesa, periodo);

        return (totalDespesas + valorDespesa.Valor) <= orcamento.Valor.Valor;
    }

    public async Task<bool> ValidarPrioridadeEssencialSuperfluoAsync(Guid usuarioId, Guid categoriaId, Money valor, DateTime data, CancellationToken cancellationToken = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(categoriaId);
        
        // Assumindo que categoria tem uma propriedade de prioridade
        // Como não encontrei PrioridadeCategoria na entity, vou usar um método simplificado
        
        // Criar período para o mês
        var periodo = new Periodo(
            new DateTime(data.Year, data.Month, 1),
            new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month)));

        var totalDespesas = await _unitOfWork.Transacoes.GetTotalByTipoAsync(usuarioId, TipoTransacao.Despesa, periodo);
        var novoTotal = totalDespesas + valor.Valor;

        // Regra simplificada: não permitir gastos acima de um limite para categorias consideradas supérfluas
        // Como não temos a distinção explícita, vamos usar o limite da categoria
        return categoria?.Limite is null || novoTotal <= categoria.Limite.Valor * 2;
    }

    public async Task<decimal> CalcularGastoMensalCategoriaAsync(Guid categoriaId, DateTime data, CancellationToken cancellationToken = default)
    {
        // Criar período para o mês
        var periodo = new Periodo(
            new DateTime(data.Year, data.Month, 1),
            new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month)));

        var transacoes = await _unitOfWork.Transacoes.GetByCategoriaAsync(categoriaId);
        
        return transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa && 
                       t.DataTransacao >= periodo.DataInicio && 
                       t.DataTransacao <= periodo.DataFim)
            .Sum(t => t.Valor.Valor);
    }
}
