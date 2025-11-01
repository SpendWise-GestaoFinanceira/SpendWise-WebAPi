using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Commands.FechamentoMensal;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.Enums;

namespace SpendWise.Application.Handlers.FechamentoMensal;

public class FecharMesCommandHandler : IRequestHandler<FecharMesCommand, FechamentoMensalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FecharMesCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<FechamentoMensalDto> Handle(FecharMesCommand request, CancellationToken cancellationToken)
    {
        // Validar se o usuário existe
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId);
        if (usuario == null)
            throw new ArgumentException("Usuário não encontrado");

        // Verificar se o mês já está fechado
        var fechamentoExistente = await _unitOfWork.FechamentosMensais
            .GetByUsuarioEAnoMesAsync(request.UsuarioId, request.AnoMes);
        
        if (fechamentoExistente != null)
            throw new InvalidOperationException($"O mês {request.AnoMes} já está fechado");

        // Calcular totais do mês
        var anoMesData = DateTime.ParseExact(request.AnoMes + "-01", "yyyy-MM-dd", null);
        var inicioMes = new DateTime(anoMesData.Year, anoMesData.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var transacoes = await _unitOfWork.Transacoes.GetByUsuarioIdAsync(request.UsuarioId);
        var transacoesMes = transacoes.Where(t => 
            t.DataTransacao >= inicioMes && t.DataTransacao <= fimMes);

        var totalReceitas = transacoesMes
            .Where(t => t.Tipo == TipoTransacao.Receita)
            .Sum(t => t.Valor.Valor);

        var totalDespesas = transacoesMes
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor.Valor);

        // Criar fechamento
        var fechamento = new Domain.Entities.FechamentoMensal(
            request.UsuarioId,
            request.AnoMes,
            totalReceitas,
            totalDespesas,
            $"Fechamento automático do mês {request.AnoMes}"
        );

        await _unitOfWork.FechamentosMensais.AddAsync(fechamento);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<FechamentoMensalDto>(fechamento);
    }
}
