using MediatR;
using SpendWise.Application.DTOs.Relatorios;

namespace SpendWise.Application.Queries.Relatorios;

public record GetResumoMensalQuery(Guid UsuarioId, string AnoMes) : IRequest<ResumoMensalDto>;
