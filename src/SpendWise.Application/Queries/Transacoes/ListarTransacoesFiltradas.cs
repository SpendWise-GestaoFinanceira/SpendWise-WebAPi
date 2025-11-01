using MediatR;
using SpendWise.Application.DTOs.Transacoes;

namespace SpendWise.Application.Queries.Transacoes;

public record ListarTransacoesFiltradas(
    Guid UsuarioId,
    FiltroTransacoesDto Filtros,
    PaginacaoDto Paginacao
) : IRequest<TransacoesPaginadasDto>;
