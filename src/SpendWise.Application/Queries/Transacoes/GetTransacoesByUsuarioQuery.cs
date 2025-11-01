using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Transacoes;

public record GetTransacoesByUsuarioQuery(Guid UsuarioId) : IRequest<IEnumerable<TransacaoDto>>;