using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.Transacoes;

public class GetTransacoesByPeriodoQueryHandler : IRequestHandler<GetTransacoesByPeriodoQuery, IEnumerable<TransacaoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTransacoesByPeriodoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransacaoDto>> Handle(GetTransacoesByPeriodoQuery request, CancellationToken cancellationToken)
    {
        var periodo = new Periodo(request.DataInicio, request.DataFim);
        
        IEnumerable<Domain.Entities.Transacao> transacoes;
        
        if (request.UsuarioId.HasValue)
        {
            transacoes = await _unitOfWork.Transacoes.GetByPeriodoAsync(request.UsuarioId.Value, periodo);
        }
        else
        {
            // Se não especificar usuário, buscar todas (admin)
            var todasTransacoes = await _unitOfWork.Transacoes.GetAllAsync();
            transacoes = todasTransacoes.Where(t => 
                t.DataTransacao >= request.DataInicio && 
                t.DataTransacao <= request.DataFim);
        }
        
        return _mapper.Map<IEnumerable<TransacaoDto>>(transacoes);
    }
}