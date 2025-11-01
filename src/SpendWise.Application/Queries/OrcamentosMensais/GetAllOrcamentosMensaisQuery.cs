using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.OrcamentosMensais;

public record GetAllOrcamentosMensaisQuery() : IRequest<IEnumerable<OrcamentoMensalDto>>;
