using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Transacoes;

public record GetAllTransacoesQuery() : IRequest<IEnumerable<TransacaoDto>>;