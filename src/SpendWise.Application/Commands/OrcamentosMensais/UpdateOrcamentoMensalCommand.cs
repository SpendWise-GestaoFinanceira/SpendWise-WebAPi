using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Commands.OrcamentosMensais;

public record UpdateOrcamentoMensalCommand(
    Guid Id,
    Money Valor,
    Guid UsuarioId
) : IRequest<OrcamentoMensalDto>;