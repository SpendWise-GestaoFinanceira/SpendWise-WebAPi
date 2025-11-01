using MediatR;

namespace SpendWise.Application.Commands.FechamentoMensal;

public record ReabrirMesCommand(
    Guid UsuarioId,
    string AnoMes
) : IRequest<bool>;
