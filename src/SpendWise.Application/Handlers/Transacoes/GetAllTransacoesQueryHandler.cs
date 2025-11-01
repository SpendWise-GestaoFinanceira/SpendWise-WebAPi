using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Transacoes;

public class GetAllTransacoesQueryHandler : IRequestHandler<GetAllTransacoesQuery, IEnumerable<TransacaoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllTransacoesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransacaoDto>> Handle(GetAllTransacoesQuery request, CancellationToken cancellationToken)
    {
        var transacoes = await _unitOfWork.Transacoes.GetAllAsync();
        return _mapper.Map<IEnumerable<TransacaoDto>>(transacoes);
    }
}