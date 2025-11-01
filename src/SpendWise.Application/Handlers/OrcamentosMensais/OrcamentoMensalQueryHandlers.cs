using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.OrcamentosMensais;
using SpendWise.Application.Services;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.OrcamentosMensais;

public class GetOrcamentoMensalByIdQueryHandler : IRequestHandler<GetOrcamentoMensalByIdQuery, OrcamentoMensalDto?>
{
    private readonly IOrcamentoMensalRepository _orcamentoMensalRepository;
    private readonly IOrcamentoCalculoService _calculoService;

    public GetOrcamentoMensalByIdQueryHandler(
        IOrcamentoMensalRepository orcamentoMensalRepository,
        IOrcamentoCalculoService calculoService)
    {
        _orcamentoMensalRepository = orcamentoMensalRepository;
        _calculoService = calculoService;
    }

    public async Task<OrcamentoMensalDto?> Handle(GetOrcamentoMensalByIdQuery request, CancellationToken cancellationToken)
    {
        var orcamentoMensal = await _orcamentoMensalRepository.GetByIdAsync(request.Id);
        if (orcamentoMensal == null)
        {
            return null;
        }

        // Usar o serviço de cálculo para obter estatísticas
        var estatisticas = await _calculoService.CalcularEstatisticasOrcamentoAsync(
            orcamentoMensal.UsuarioId, 
            orcamentoMensal.AnoMes);

        // Calcular percentuais e status
        var calculo = _calculoService.CalcularPercentuais(
            orcamentoMensal.Valor.Valor, 
            estatisticas.ValorGasto);

        return new OrcamentoMensalDto
        {
            Id = orcamentoMensal.Id,
            UsuarioId = orcamentoMensal.UsuarioId,
            AnoMes = orcamentoMensal.AnoMes,
            Valor = orcamentoMensal.Valor,
            CreatedAt = orcamentoMensal.CreatedAt,
            UpdatedAt = orcamentoMensal.UpdatedAt,
            ValorGasto = calculo.ValorGasto,
            ValorRestante = calculo.ValorRestante,
            PercentualUtilizado = calculo.PercentualUtilizado,
            Status = (DTOs.StatusOrcamento)calculo.Status,
            Categoria = calculo.Categoria,
            MensagemStatus = calculo.MensagemStatus
        };
    }
}

public class GetOrcamentosMensaisByUsuarioQueryHandler : IRequestHandler<GetOrcamentosMensaisByUsuarioQuery, IEnumerable<OrcamentoMensalDto>>
{
    private readonly IOrcamentoMensalRepository _orcamentoMensalRepository;
    private readonly IOrcamentoCalculoService _calculoService;

    public GetOrcamentosMensaisByUsuarioQueryHandler(
        IOrcamentoMensalRepository orcamentoMensalRepository,
        IOrcamentoCalculoService calculoService)
    {
        _orcamentoMensalRepository = orcamentoMensalRepository;
        _calculoService = calculoService;
    }

    public async Task<IEnumerable<OrcamentoMensalDto>> Handle(GetOrcamentosMensaisByUsuarioQuery request, CancellationToken cancellationToken)
    {
        var orcamentosMensais = await _orcamentoMensalRepository.GetByUsuarioIdAsync(request.UsuarioId);
        var result = new List<OrcamentoMensalDto>();

        foreach (var orcamento in orcamentosMensais)
        {
            // Usar o serviço de cálculo para obter estatísticas
            var estatisticas = await _calculoService.CalcularEstatisticasOrcamentoAsync(
                orcamento.UsuarioId, 
                orcamento.AnoMes);

            // Calcular percentuais e status
            var calculo = _calculoService.CalcularPercentuais(
                orcamento.Valor.Valor, 
                estatisticas.ValorGasto);

            result.Add(new OrcamentoMensalDto
            {
                Id = orcamento.Id,
                UsuarioId = orcamento.UsuarioId,
                AnoMes = orcamento.AnoMes,
                Valor = orcamento.Valor,
                CreatedAt = orcamento.CreatedAt,
                UpdatedAt = orcamento.UpdatedAt,
                ValorGasto = calculo.ValorGasto,
                ValorRestante = calculo.ValorRestante,
                PercentualUtilizado = calculo.PercentualUtilizado,
                Status = (DTOs.StatusOrcamento)calculo.Status,
                Categoria = calculo.Categoria,
                MensagemStatus = calculo.MensagemStatus
            });
        }

        return result.OrderByDescending(o => o.AnoMes);
    }
}

public class GetOrcamentoMensalByUsuarioEAnoMesQueryHandler : IRequestHandler<GetOrcamentoMensalByUsuarioEAnoMesQuery, OrcamentoMensalDto?>
{
    private readonly IOrcamentoMensalRepository _orcamentoMensalRepository;
    private readonly IOrcamentoCalculoService _calculoService;

    public GetOrcamentoMensalByUsuarioEAnoMesQueryHandler(
        IOrcamentoMensalRepository orcamentoMensalRepository,
        IOrcamentoCalculoService calculoService)
    {
        _orcamentoMensalRepository = orcamentoMensalRepository;
        _calculoService = calculoService;
    }

    public async Task<OrcamentoMensalDto?> Handle(GetOrcamentoMensalByUsuarioEAnoMesQuery request, CancellationToken cancellationToken)
    {
        var orcamentoMensal = await _orcamentoMensalRepository.GetByUsuarioEAnoMesAsync(request.UsuarioId, request.AnoMes);
        if (orcamentoMensal == null)
        {
            return null;
        }

        // Usar o serviço de cálculo para obter estatísticas
        var estatisticas = await _calculoService.CalcularEstatisticasOrcamentoAsync(
            orcamentoMensal.UsuarioId, 
            orcamentoMensal.AnoMes);

        // Calcular percentuais e status
        var calculo = _calculoService.CalcularPercentuais(
            orcamentoMensal.Valor.Valor, 
            estatisticas.ValorGasto);

        return new OrcamentoMensalDto
        {
            Id = orcamentoMensal.Id,
            UsuarioId = orcamentoMensal.UsuarioId,
            AnoMes = orcamentoMensal.AnoMes,
            Valor = orcamentoMensal.Valor,
            CreatedAt = orcamentoMensal.CreatedAt,
            UpdatedAt = orcamentoMensal.UpdatedAt,
            ValorGasto = calculo.ValorGasto,
            ValorRestante = calculo.ValorRestante,
            PercentualUtilizado = calculo.PercentualUtilizado,
            Status = (DTOs.StatusOrcamento)calculo.Status,
            Categoria = calculo.Categoria,
            MensagemStatus = calculo.MensagemStatus
        };
    }
}
