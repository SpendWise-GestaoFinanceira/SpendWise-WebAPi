using AutoMapper;
using MediatR;
using SpendWise.Application.Common;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Transacoes;

public class GetTransacoesAdvancedQueryHandler : IRequestHandler<GetTransacoesAdvancedQuery, PaginatedResponse<TransacaoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTransacoesAdvancedQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<TransacaoDto>> Handle(GetTransacoesAdvancedQuery request, CancellationToken cancellationToken)
    {
        // Buscar transações com filtros
        var transacoes = await _unitOfWork.Transacoes.GetAdvancedFilteredAsync(
            usuarioId: request.UsuarioId,
            dataInicio: request.DataInicio,
            dataFim: request.DataFim,
            valorMinimo: request.ValorMinimo,
            valorMaximo: request.ValorMaximo,
            categoriaId: request.CategoriaId,
            tipo: request.Tipo,
            descricao: request.Descricao,
            observacoes: request.Observacoes,
            orderBy: request.OrderBy ?? "DataTransacao",
            ascending: request.Ascending,
            skip: request.GetSkip(),
            take: request.GetValidPageSize()
        );

        // Contar total para paginação
        var totalCount = await _unitOfWork.Transacoes.CountAdvancedFilteredAsync(
            usuarioId: request.UsuarioId,
            dataInicio: request.DataInicio,
            dataFim: request.DataFim,
            valorMinimo: request.ValorMinimo,
            valorMaximo: request.ValorMaximo,
            categoriaId: request.CategoriaId,
            tipo: request.Tipo,
            descricao: request.Descricao,
            observacoes: request.Observacoes
        );

        var transacaoDtos = _mapper.Map<List<TransacaoDto>>(transacoes);

        return new PaginatedResponse<TransacaoDto>
        {
            Items = transacaoDtos,
            TotalItems = totalCount,
            Page = request.GetValidPage(),
            PageSize = request.GetValidPageSize()
        };
    }
}
