using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Transacoes;

public record GetTransacaoByIdQuery(Guid Id) : IRequest<TransacaoDto?>;