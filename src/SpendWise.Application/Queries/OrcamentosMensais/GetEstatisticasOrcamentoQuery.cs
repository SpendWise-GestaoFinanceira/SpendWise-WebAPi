using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.OrcamentosMensais;

public record GetEstatisticasOrcamentoQuery(
    Guid UsuarioId,
    string AnoMes
) : IRequest<OrcamentoCalculoResultado>;
