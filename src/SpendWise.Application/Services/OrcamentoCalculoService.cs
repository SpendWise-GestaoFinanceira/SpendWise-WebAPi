using SpendWise.Application.Services;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Services;

public class OrcamentoCalculoService : IOrcamentoCalculoService
{
    private readonly ITransacaoRepository _transacaoRepository;

    public OrcamentoCalculoService(ITransacaoRepository transacaoRepository)
    {
        _transacaoRepository = transacaoRepository;
    }

    public async Task<OrcamentoCalculoResultado> CalcularEstatisticasOrcamentoAsync(Guid usuarioId, string anoMes)
    {
        var periodo = CriarPeriodo(anoMes);
        
        // Buscar total de despesas do período
        var valorGasto = await _transacaoRepository.GetTotalByTipoAsync(
            usuarioId, 
            TipoTransacao.Despesa, 
            periodo);

        // Como não temos o valor do orçamento aqui, retornamos só o valor gasto
        // O valor do orçamento será passado posteriormente
        return new OrcamentoCalculoResultado
        {
            ValorGasto = valorGasto
        };
    }

    public async Task<IEnumerable<OrcamentoCalculoResultado>> CalcularEstatisticasMultiplosOrcamentosAsync(
        Guid usuarioId, 
        IEnumerable<string> anoMeses)
    {
        var resultados = new List<OrcamentoCalculoResultado>();

        foreach (var anoMes in anoMeses)
        {
            var resultado = await CalcularEstatisticasOrcamentoAsync(usuarioId, anoMes);
            resultados.Add(resultado);
        }

        return resultados;
    }

    public OrcamentoCalculoResultado CalcularPercentuais(decimal valorOrcamento, decimal valorGasto)
    {
        var valorRestante = valorOrcamento - valorGasto;
        var percentualUtilizado = valorOrcamento > 0 
            ? Math.Round((valorGasto / valorOrcamento) * 100, 2) 
            : 0;

        var status = DeterminarStatus(percentualUtilizado);
        var categoria = DeterminarCategoria(percentualUtilizado);
        var mensagem = GerarMensagemStatus(percentualUtilizado, valorRestante);

        return new OrcamentoCalculoResultado
        {
            ValorOrcamento = valorOrcamento,
            ValorGasto = valorGasto,
            ValorRestante = valorRestante,
            PercentualUtilizado = percentualUtilizado,
            Status = status,
            Categoria = categoria,
            MensagemStatus = mensagem
        };
    }

    private static StatusOrcamento DeterminarStatus(decimal percentual)
    {
        return percentual switch
        {
            <= 80 => StatusOrcamento.Dentro,
            <= 95 => StatusOrcamento.Atencao,
            <= 100 => StatusOrcamento.Alerta,
            _ => StatusOrcamento.Excedido
        };
    }

    private static string DeterminarCategoria(decimal percentual)
    {
        return percentual switch
        {
            <= 50 => "Excelente",
            <= 80 => "Bom",
            <= 95 => "Atenção",
            <= 100 => "Alerta",
            _ => "Excedido"
        };
    }

    private static string GerarMensagemStatus(decimal percentual, decimal valorRestante)
    {
        return percentual switch
        {
            <= 50 => "Orçamento bem controlado!",
            <= 80 => "Orçamento dentro do esperado.",
            <= 95 => "Atenção: Próximo do limite do orçamento.",
            <= 100 => "Alerta: Orçamento quase esgotado!",
            _ => $"Orçamento excedido em R$ {Math.Abs(valorRestante):N2}!"
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
