using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.OrcamentosMensais;

public record GetOrcamentosMensaisByUsuarioQuery(
    Guid UsuarioId
) : IRequest<IEnumerable<OrcamentoMensalDto>>;
