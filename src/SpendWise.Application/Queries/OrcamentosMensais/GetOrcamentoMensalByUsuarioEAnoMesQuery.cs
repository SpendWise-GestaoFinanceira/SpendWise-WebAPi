using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.OrcamentosMensais;

public record GetOrcamentoMensalByUsuarioEAnoMesQuery(
    Guid UsuarioId,
    string AnoMes
) : IRequest<OrcamentoMensalDto?>;
