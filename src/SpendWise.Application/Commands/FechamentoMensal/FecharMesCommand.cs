using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Commands.FechamentoMensal;

public record FecharMesCommand(
    Guid UsuarioId,
    string AnoMes
) : IRequest<FechamentoMensalDto>;
