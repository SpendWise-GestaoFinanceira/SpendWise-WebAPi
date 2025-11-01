using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Transacoes;

public class GetTransacoesByCategoriaQueryHandler : IRequestHandler<GetTransacoesByCategoriaQuery, IEnumerable<TransacaoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTransacoesByCategoriaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransacaoDto>> Handle(GetTransacoesByCategoriaQuery request, CancellationToken cancellationToken)
    {
        var transacoes = await _unitOfWork.Transacoes.GetByCategoriaAsync(request.CategoriaId);
        return _mapper.Map<IEnumerable<TransacaoDto>>(transacoes);
    }
}