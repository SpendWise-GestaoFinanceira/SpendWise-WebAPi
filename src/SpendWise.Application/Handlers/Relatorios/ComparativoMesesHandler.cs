using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SpendWise.Application.DTOs.Relatorios;
using SpendWise.Application.Queries.Relatorios;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Relatorios;

public class ComparativoMesesHandler : IRequestHandler<ComparativoMesesQuery, ComparativoMesesResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ComparativoMesesHandler> _logger;

    public ComparativoMesesHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ComparativoMesesHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ComparativoMesesResponseDto> Handle(ComparativoMesesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando comparativo de meses para usuário {UsuarioId} de {AnoInicio}-{MesInicio} até {AnoFim}-{MesFim}",
            request.UsuarioId, request.AnoInicio, request.MesInicio, request.AnoFim, request.MesFim);

        var response = new ComparativoMesesResponseDto();

        // Gerar lista de meses no período
        var meses = GerarListaMeses(request.AnoInicio, request.MesInicio, request.AnoFim, request.MesFim);
        
        // Buscar orçamentos mensais do período
        var orcamentos = await _unitOfWork.OrcamentosMensais.BuscarPorPeriodoAsync(
            request.UsuarioId, 
            request.AnoInicio, 
            request.MesInicio, 
            request.AnoFim, 
            request.MesFim,
            cancellationToken);

        // Buscar transações do período
        var transacoes = await _unitOfWork.Transacoes.BuscarPorPeriodoComCategoriasAsync(
            request.UsuarioId,
            new DateTime(request.AnoInicio, request.MesInicio, 1),
            new DateTime(request.AnoFim, request.MesFim, DateTime.DaysInMonth(request.AnoFim, request.MesFim)),
            request.CategoriaIds,
            cancellationToken);

        // Processar resumos mensais
        response.ResumosMensais = ProcessarResumosMensais(meses, orcamentos, transacoes, request.IncluirDetalhes);

        // Calcular estatísticas comparativas
        response.Estatisticas = CalcularEstatisticas(response.ResumosMensais);

        // Calcular tendências
        response.Tendencias = CalcularTendencias(response.ResumosMensais);

        _logger.LogInformation("Comparativo de meses gerado com sucesso para {QuantidadeMeses} meses",
            response.ResumosMensais.Count);

        return response;
    }

    private List<(int Ano, int Mes)> GerarListaMeses(int anoInicio, int mesInicio, int anoFim, int mesFim)
    {
        var meses = new List<(int Ano, int Mes)>();
        var dataAtual = new DateTime(anoInicio, mesInicio, 1);
        var dataFim = new DateTime(anoFim, mesFim, 1);

        while (dataAtual <= dataFim)
        {
            meses.Add((dataAtual.Year, dataAtual.Month));
            dataAtual = dataAtual.AddMonths(1);
        }

        return meses;
    }

    private List<ResumoMensalComparativoDto> ProcessarResumosMensais(
        List<(int Ano, int Mes)> meses,
        IEnumerable<Domain.Entities.OrcamentoMensal> orcamentos,
        IEnumerable<Domain.Entities.Transacao> transacoes,
        bool incluirDetalhes)
    {
        var resumos = new List<ResumoMensalComparativoDto>();

        foreach (var (ano, mes) in meses)
        {
            var anoMesString = $"{ano:0000}-{mes:00}";
            var orcamento = orcamentos.FirstOrDefault(o => o.AnoMes == anoMesString);
            var transacoesMes = transacoes.Where(t => t.DataTransacao.Year == ano && t.DataTransacao.Month == mes).ToList();

            var totalReceitas = transacoesMes
                .Where(t => t.Tipo == TipoTransacao.Receita)
                .Sum(t => t.Valor.Valor);

            var totalDespesas = transacoesMes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .Sum(t => t.Valor.Valor);

            var saldoLiquido = totalReceitas - totalDespesas;
            var percentualEconomia = totalReceitas > 0 ? (saldoLiquido / totalReceitas) * 100 : 0;

            var resumo = new ResumoMensalComparativoDto
            {
                Ano = ano,
                Mes = mes,
                AnoMes = $"{ano:0000}-{mes:00}",
                NomeMes = ObterNomeMes(mes, ano),
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                SaldoLiquido = saldoLiquido,
                PercentualEconomia = percentualEconomia,
                QuantidadeTransacoes = transacoesMes.Count,
                MesFechado = false // Por enquanto, pode ser implementado mais tarde
            };

            if (incluirDetalhes)
            {
                resumo.DetalhesCategorias = ProcessarDetalhesCategorias(transacoesMes);
            }

            resumos.Add(resumo);
        }

        return resumos.OrderBy(r => r.Ano).ThenBy(r => r.Mes).ToList();
    }

    private List<ResumoCategoriaDto> ProcessarDetalhesCategorias(List<Domain.Entities.Transacao> transacoes)
    {
        return transacoes
            .GroupBy(t => new { t.CategoriaId, t.Categoria.Nome })
            .Select(g => new ResumoCategoriaDto
            {
                CategoriaId = g.Key.CategoriaId,
                NomeCategoria = g.Key.Nome,
                ValorReceitas = g.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor.Valor),
                ValorDespesas = g.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor.Valor),
                SaldoCategoria = g.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor.Valor) -
                                g.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor.Valor),
                QuantidadeTransacoes = g.Count()
            })
            .OrderBy(c => c.NomeCategoria)
            .ToList();
    }

    private EstatisticasComparativasDto CalcularEstatisticas(List<ResumoMensalComparativoDto> resumos)
    {
        if (!resumos.Any())
            return new EstatisticasComparativasDto();

        var melhorMes = resumos.OrderByDescending(r => r.SaldoLiquido).First();
        var piorMes = resumos.OrderBy(r => r.SaldoLiquido).First();

        var primeiroMes = resumos.OrderBy(r => r.Ano).ThenBy(r => r.Mes).First();
        var ultimoMes = resumos.OrderByDescending(r => r.Ano).ThenByDescending(r => r.Mes).First();

        var variacaoReceitas = primeiroMes.TotalReceitas > 0
            ? ((ultimoMes.TotalReceitas - primeiroMes.TotalReceitas) / primeiroMes.TotalReceitas) * 100
            : 0;

        var variacaoDespesas = primeiroMes.TotalDespesas > 0
            ? ((ultimoMes.TotalDespesas - primeiroMes.TotalDespesas) / primeiroMes.TotalDespesas) * 100
            : 0;

        return new EstatisticasComparativasDto
        {
            MediaReceitas = resumos.Average(r => r.TotalReceitas),
            MediaDespesas = resumos.Average(r => r.TotalDespesas),
            MediaSaldo = resumos.Average(r => r.SaldoLiquido),
            MelhorMes = melhorMes,
            PiorMes = piorMes,
            MaiorReceita = resumos.Max(r => r.TotalReceitas),
            MaiorDespesa = resumos.Max(r => r.TotalDespesas),
            VariacaoPercentualReceitas = variacaoReceitas,
            VariacaoPercentualDespesas = variacaoDespesas
        };
    }

    private List<TendenciaDto> CalcularTendencias(List<ResumoMensalComparativoDto> resumos)
    {
        var tendencias = new List<TendenciaDto>();

        if (resumos.Count < 2)
            return tendencias;

        // Tendência de receitas
        var tendenciaReceitas = CalcularTendenciaIndicador(
            resumos.Select(r => r.TotalReceitas).ToList(),
            "Receitas");
        tendencias.Add(tendenciaReceitas);

        // Tendência de despesas
        var tendenciaDespesas = CalcularTendenciaIndicador(
            resumos.Select(r => r.TotalDespesas).ToList(),
            "Despesas");
        tendencias.Add(tendenciaDespesas);

        // Tendência de saldo
        var tendenciaSaldo = CalcularTendenciaIndicador(
            resumos.Select(r => r.SaldoLiquido).ToList(),
            "Saldo");
        tendencias.Add(tendenciaSaldo);

        return tendencias;
    }

    private TendenciaDto CalcularTendenciaIndicador(List<decimal> valores, string nomeIndicador)
    {
        if (valores.Count < 2)
        {
            return new TendenciaDto
            {
                Indicador = nomeIndicador,
                Tendencia = "Insuficiente",
                VariacaoPercentual = 0,
                Descricao = "Dados insuficientes para análise de tendência"
            };
        }

        var primeiro = valores.First();
        var ultimo = valores.Last();

        var variacao = primeiro != 0 ? ((ultimo - primeiro) / Math.Abs(primeiro)) * 100 : 0;

        string tendencia;
        string descricao;

        if (Math.Abs(variacao) < 5)
        {
            tendencia = "Estável";
            descricao = $"{nomeIndicador} mantêm-se relativamente estáveis no período";
        }
        else if (variacao > 0)
        {
            tendencia = "Crescente";
            descricao = $"{nomeIndicador} apresentam tendência de crescimento";
        }
        else
        {
            tendencia = "Decrescente";
            descricao = $"{nomeIndicador} apresentam tendência de redução";
        }

        return new TendenciaDto
        {
            Indicador = nomeIndicador,
            Tendencia = tendencia,
            VariacaoPercentual = variacao,
            Descricao = descricao
        };
    }

    private string ObterNomeMes(int mes, int ano)
    {
        var nomesMeses = new[]
        {
            "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
            "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
        };

        return $"{nomesMeses[mes - 1]} {ano}";
    }
}
