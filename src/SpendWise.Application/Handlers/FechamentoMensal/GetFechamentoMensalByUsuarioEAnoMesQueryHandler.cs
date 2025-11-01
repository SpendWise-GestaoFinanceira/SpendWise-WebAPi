using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.FechamentoMensal;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.FechamentoMensal;

public class GetFechamentoMensalByUsuarioEAnoMesQueryHandler : IRequestHandler<GetFechamentoMensalByUsuarioEAnoMesQuery, FechamentoMensalDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFechamentoMensalByUsuarioEAnoMesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<FechamentoMensalDto?> Handle(GetFechamentoMensalByUsuarioEAnoMesQuery request, CancellationToken cancellationToken)
    {
        var fechamento = await _unitOfWork.FechamentosMensais
            .GetByUsuarioEAnoMesAsync(request.UsuarioId, request.AnoMes);
            
        return fechamento != null ? _mapper.Map<FechamentoMensalDto>(fechamento) : null;
    }
}
