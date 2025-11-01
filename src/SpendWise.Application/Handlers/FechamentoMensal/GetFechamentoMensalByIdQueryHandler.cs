using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.FechamentoMensal;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.FechamentoMensal;

public class GetFechamentoMensalByIdQueryHandler : IRequestHandler<GetFechamentoMensalByIdQuery, FechamentoMensalDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFechamentoMensalByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<FechamentoMensalDto?> Handle(GetFechamentoMensalByIdQuery request, CancellationToken cancellationToken)
    {
        var fechamento = await _unitOfWork.FechamentosMensais.GetByIdAsync(request.Id);
        return fechamento != null ? _mapper.Map<FechamentoMensalDto>(fechamento) : null;
    }
}
