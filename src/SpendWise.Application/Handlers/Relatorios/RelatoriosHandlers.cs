using MediatR;
using SpendWise.Application.DTOs.Relatorios;
using SpendWise.Application.Queries.Relatorios;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Utils;

namespace SpendWise.Application.Handlers.Relatorios;

public class GetRelatorioCategoriasQueryHandler : IRequestHandler<GetRelatorioCategoriasQuery, IEnumerable<CategoriaSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRelatorioCategoriasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoriaSummaryDto>> Handle(GetRelatorioCategoriasQuery request, CancellationToken cancellationToken)
    {
        // Implementação básica - pode ser expandida
        var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);
        
        var resultado = new List<CategoriaSummaryDto>();
        
        foreach (var categoria in categorias)
        {
            var transacoes = await _unitOfWork.Transacoes.GetByCategoriaAsync(categoria.Id);
            var transacoesPeriodo = transacoes.Where(t => 
                t.DataTransacao.Date >= request.DataInicio.Date && 
                t.DataTransacao.Date <= request.DataFim.Date);
            
            var totalGasto = transacoesPeriodo.Sum(t => t.Valor.Valor);
            
            resultado.Add(new CategoriaSummaryDto
            {
                CategoriaId = categoria.Id,
                NomeCategoria = categoria.Nome,
                TotalGasto = totalGasto,
                LimiteCategoria = categoria.Limite?.Valor ?? 0,
                PercentualLimite = categoria.Limite?.Valor > 0 ? (totalGasto / categoria.Limite.Valor) * 100 : 0,
                QuantidadeTransacoes = transacoesPeriodo.Count(),
                StatusLimite = categoria.VerificarStatusLimite(totalGasto).ToString()
            });
        }
        
        return resultado.OrderByDescending(r => r.TotalGasto);
    }
}

public class GetEvolucaoGastosQueryHandler : IRequestHandler<GetEvolucaoGastosQuery, EvolucaoGastosDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEvolucaoGastosQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EvolucaoGastosDto> Handle(GetEvolucaoGastosQuery request, CancellationToken cancellationToken)
    {
        var dataFim = DateTime.Now;
        var dataInicio = dataFim.AddMonths(-11); // Últimos 12 meses
        
        var transacoes = await _unitOfWork.Transacoes.GetByUsuarioIdAsync(request.UsuarioId);
        var transacoesPeriodo = transacoes.Where(t => 
            t.DataTransacao >= dataInicio && t.DataTransacao <= dataFim);
        
        var gastosPorMes = new List<MesGastoDto>();
        
        for (int i = 0; i < 12; i++)
        {
            var mesAno = dataInicio.AddMonths(i);
            var transacoesMes = transacoesPeriodo.Where(t => 
                t.DataTransacao.Year == mesAno.Year && 
                t.DataTransacao.Month == mesAno.Month);
            
            var totalGasto = transacoesMes.Sum(t => t.Valor.Valor);
            
            gastosPorMes.Add(new MesGastoDto
            {
                AnoMes = $"{mesAno.Year:0000}-{mesAno.Month:00}",
                TotalGasto = totalGasto,
                Orcamento = 0 // Pode ser expandido para incluir orçamento do mês
            });
        }
        
        var valores = gastosPorMes.Select(g => g.TotalGasto).Where(v => v > 0).ToList();
        var mediaMensal = valores.Any() ? valores.Average() : 0;
        var maiorGasto = valores.Any() ? valores.Max() : 0;
        var menorGasto = valores.Any() ? valores.Min() : 0;
        
        // Calcular tendência simples: comparar últimos 3 meses com 3 anteriores
        var ultimosTres = gastosPorMes.TakeLast(3).Sum(g => g.TotalGasto) / 3;
        var anterioresTres = gastosPorMes.Skip(6).Take(3).Sum(g => g.TotalGasto) / 3;
        
        string tendencia = "Estável";
        if (ultimosTres > anterioresTres * 1.1m) tendencia = "Crescente";
        else if (ultimosTres < anterioresTres * 0.9m) tendencia = "Decrescente";
        
        return new EvolucaoGastosDto
        {
            UltimosDoozeMeses = gastosPorMes,
            MediaMensal = mediaMensal,
            MaiorGasto = maiorGasto,
            MenorGasto = menorGasto,
            TendenciaGeral = tendencia
        };
    }
}

public class GetTopCategoriasQueryHandler : IRequestHandler<GetTopCategoriasQuery, IEnumerable<TopCategoriaDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTopCategoriasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TopCategoriaDto>> Handle(GetTopCategoriasQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);
        var transacoes = await _unitOfWork.Transacoes.GetByUsuarioIdAsync(request.UsuarioId);
        
        var transacoesPeriodo = transacoes.Where(t => 
            t.DataTransacao.Date >= request.DataInicio.Date && 
            t.DataTransacao.Date <= request.DataFim.Date);
        
        var totalGeral = transacoesPeriodo.Sum(t => t.Valor.Valor);
        
        var categoriasSummary = new List<TopCategoriaDto>();
        
        foreach (var categoria in categorias)
        {
            var transacoesCategoria = transacoesPeriodo.Where(t => t.CategoriaId == categoria.Id);
            var totalCategoria = transacoesCategoria.Sum(t => t.Valor.Valor);
            
            if (totalCategoria > 0)
            {
                categoriasSummary.Add(new TopCategoriaDto
                {
                    CategoriaId = categoria.Id,
                    NomeCategoria = categoria.Nome,
                    TotalGasto = totalCategoria,
                    PercentualDoTotal = totalGeral > 0 ? (totalCategoria / totalGeral) * 100 : 0
                });
            }
        }
        
        var topCategorias = categoriasSummary
            .OrderByDescending(c => c.TotalGasto)
            .Take(request.Top)
            .Select((categoria, index) => 
            {
                categoria.Posicao = index + 1;
                return categoria;
            })
            .ToList();
        
        return topCategorias;
    }
}

public class GetOrcadoVsRealizadoQueryHandler : IRequestHandler<GetOrcadoVsRealizadoQuery, OrcadoVsRealizadoDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrcadoVsRealizadoQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrcadoVsRealizadoDto> Handle(GetOrcadoVsRealizadoQuery request, CancellationToken cancellationToken)
    {
        var periodo = DateUtils.GetPeriodoFromAnoMes(request.AnoMes);
        
        // Buscar orçamentos do mês
        var orcamentos = await _unitOfWork.OrcamentosMensais.GetByUsuarioIdAsync(request.UsuarioId);
        var orcamentoMes = orcamentos.FirstOrDefault(o => 
            o.AnoMes == request.AnoMes);
        
        // Buscar transações do mês
        var transacoes = await _unitOfWork.Transacoes.GetByUsuarioIdAsync(request.UsuarioId);
        var transacoesMes = transacoes.Where(t => 
            t.DataTransacao.Date >= periodo.DataInicio.Date && 
            t.DataTransacao.Date <= periodo.DataFim.Date);
        
        var gastoRealizado = transacoesMes.Sum(t => t.Valor.Valor);
        var orcamentoTotal = orcamentoMes?.Valor?.Valor ?? 0;
        var diferenca = orcamentoTotal - gastoRealizado;
        var percentualRealizado = orcamentoTotal > 0 ? (gastoRealizado / orcamentoTotal) * 100 : 0;
        
        // Buscar categorias para detalhamento
        var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);
        var categoriaDetalhes = new List<CategoriaOrcadoVsRealizadoDto>();
        
        foreach (var categoria in categorias)
        {
            var transacoesCategoria = transacoesMes.Where(t => t.CategoriaId == categoria.Id);
            var gastoCategoria = transacoesCategoria.Sum(t => t.Valor.Valor);
            var limiteCategoria = categoria.Limite?.Valor ?? 0;
            
            if (gastoCategoria > 0 || limiteCategoria > 0)
            {
                categoriaDetalhes.Add(new CategoriaOrcadoVsRealizadoDto
                {
                    CategoriaId = categoria.Id,
                    NomeCategoria = categoria.Nome,
                    LimiteOrcado = limiteCategoria,
                    GastoRealizado = gastoCategoria,
                    Diferenca = limiteCategoria - gastoCategoria,
                    PercentualRealizado = limiteCategoria > 0 ? (gastoCategoria / limiteCategoria) * 100 : 0
                });
            }
        }
        
        return new OrcadoVsRealizadoDto
        {
            AnoMes = request.AnoMes,
            OrcamentoTotal = orcamentoTotal,
            GastoRealizado = gastoRealizado,
            Diferenca = diferenca,
            PercentualRealizado = percentualRealizado,
            CategoriaDetalhes = categoriaDetalhes.OrderByDescending(c => c.GastoRealizado).ToList()
        };
    }
}
