using MediatR;
using SpendWise.Application.DTOs.Relatorios;
using SpendWise.Application.Queries.Relatorios;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Utils;

namespace SpendWise.Application.Handlers.Relatorios;

public class GetResumoMensalQueryHandler : IRequestHandler<GetResumoMensalQuery, ResumoMensalDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetResumoMensalQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResumoMensalDto> Handle(GetResumoMensalQuery request, CancellationToken cancellationToken)
    {
        var periodo = DateUtils.GetPeriodoFromAnoMes(request.AnoMes);
        
        // Buscar totais de receitas e despesas
        var totalReceitas = await _unitOfWork.Transacoes.GetTotalByTipoAsync(
            request.UsuarioId, TipoTransacao.Receita, periodo);
            
        var totalDespesas = await _unitOfWork.Transacoes.GetTotalByTipoAsync(
            request.UsuarioId, TipoTransacao.Despesa, periodo);

        // Buscar orçamento planejado
        var orcamento = await _unitOfWork.OrcamentosMensais.GetByUsuarioEAnoMesAsync(
            request.UsuarioId, request.AnoMes);

        // Contar transações
        var transacoes = await _unitOfWork.Transacoes.GetByUsuarioIdAsync(request.UsuarioId);
        var transacoesMes = transacoes.Where(t => 
            t.DataTransacao.Date >= periodo.DataInicio && 
            t.DataTransacao.Date <= periodo.DataFim).Count();

        var saldo = totalReceitas - totalDespesas;
        var orcamentoPlanejado = orcamento?.Valor?.Valor ?? 0;
        var percentualGasto = orcamentoPlanejado > 0 ? (totalDespesas / orcamentoPlanejado) * 100 : 0;

        string statusMes;
        if (saldo > 0) statusMes = "Superavit";
        else if (saldo < 0) statusMes = "Deficit";
        else statusMes = "Equilibrado";

        return new ResumoMensalDto
        {
            AnoMes = request.AnoMes,
            TotalReceitas = totalReceitas,
            TotalDespesas = totalDespesas,
            Saldo = saldo,
            OrcamentoPlanejado = orcamentoPlanejado,
            PercentualGasto = percentualGasto,
            QuantidadeTransacoes = transacoesMes,
            StatusMes = statusMes
        };
    }
}
