using MediatR;
using SpendWise.Application.DTOs.Relatorios;

namespace SpendWise.Application.Queries.Relatorios;

public record GetRelatorioCategoriasQuery(Guid UsuarioId, DateTime DataInicio, DateTime DataFim) : IRequest<IEnumerable<CategoriaSummaryDto>>;
public record GetEvolucaoGastosQuery(Guid UsuarioId) : IRequest<EvolucaoGastosDto>;
public record GetTopCategoriasQuery(Guid UsuarioId, DateTime DataInicio, DateTime DataFim, int Top) : IRequest<IEnumerable<TopCategoriaDto>>;
public record GetOrcadoVsRealizadoQuery(Guid UsuarioId, string AnoMes) : IRequest<OrcadoVsRealizadoDto>;
