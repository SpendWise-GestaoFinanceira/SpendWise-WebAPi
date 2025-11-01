using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Transacoes;

public record GetTransacoesByCategoriaQuery(Guid CategoriaId) : IRequest<IEnumerable<TransacaoDto>>;