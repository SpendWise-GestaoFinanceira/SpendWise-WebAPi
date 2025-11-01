using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.FechamentoMensal;

public record GetFechamentoMensalByUsuarioEAnoMesQuery(
    Guid UsuarioId,
    string AnoMes
) : IRequest<FechamentoMensalDto?>;
