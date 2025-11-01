using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Transacoes;

public class GetTransacaoByIdQueryHandler : IRequestHandler<GetTransacaoByIdQuery, TransacaoDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTransacaoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TransacaoDto?> Handle(GetTransacaoByIdQuery request, CancellationToken cancellationToken)
    {
        var transacao = await _unitOfWork.Transacoes.GetByIdAsync(request.Id);
        
        if (transacao == null)
            return null;

        return _mapper.Map<TransacaoDto>(transacao);
    }
}