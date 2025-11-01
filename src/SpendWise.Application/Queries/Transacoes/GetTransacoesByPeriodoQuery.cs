using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Transacoes;

public record GetTransacoesByPeriodoQuery(
    DateTime DataInicio,
    DateTime DataFim,
    Guid? UsuarioId = null
) : IRequest<IEnumerable<TransacaoDto>>;