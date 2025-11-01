using MediatR;

namespace SpendWise.Application.Commands.OrcamentosMensais;

public record DeleteOrcamentoMensalCommand(
    Guid Id,
    Guid UsuarioId
) : IRequest<bool>;