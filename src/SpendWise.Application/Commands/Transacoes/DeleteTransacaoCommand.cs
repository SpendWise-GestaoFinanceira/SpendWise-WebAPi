using MediatR;

namespace SpendWise.Application.Commands.Transacoes;

public record DeleteTransacaoCommand(Guid Id) : IRequest<bool>;