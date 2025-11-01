using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.OrcamentosMensais;

public record GetEstatisticasCategoriasQuery(Guid UsuarioId, string AnoMes) : IRequest<EstatisticasCategoriasDto>;
