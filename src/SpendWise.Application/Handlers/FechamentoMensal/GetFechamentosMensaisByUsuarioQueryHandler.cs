using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.FechamentoMensal;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.FechamentoMensal;

public class GetFechamentosMensaisByUsuarioQueryHandler : IRequestHandler<GetFechamentosMensaisByUsuarioQuery, IEnumerable<FechamentoMensalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFechamentosMensaisByUsuarioQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FechamentoMensalDto>> Handle(GetFechamentosMensaisByUsuarioQuery request, CancellationToken cancellationToken)
    {
        var fechamentos = await _unitOfWork.FechamentosMensais.GetByUsuarioIdAsync(request.UsuarioId);
        return _mapper.Map<IEnumerable<FechamentoMensalDto>>(fechamentos);
    }
}
