using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Transacoes;

public class GetTransacoesByUsuarioQueryHandler : IRequestHandler<GetTransacoesByUsuarioQuery, IEnumerable<TransacaoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTransacoesByUsuarioQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransacaoDto>> Handle(GetTransacoesByUsuarioQuery request, CancellationToken cancellationToken)
    {
        var transacoes = await _unitOfWork.Transacoes.GetByUsuarioIdAsync(request.UsuarioId);
        return _mapper.Map<IEnumerable<TransacaoDto>>(transacoes);
    }
}