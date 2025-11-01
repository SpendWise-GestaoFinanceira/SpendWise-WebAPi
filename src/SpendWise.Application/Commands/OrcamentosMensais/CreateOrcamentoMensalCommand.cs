using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Commands.OrcamentosMensais;

public record CreateOrcamentoMensalCommand(
    Guid UsuarioId,
    string AnoMes,
    Money Valor
) : IRequest<OrcamentoMensalDto>;