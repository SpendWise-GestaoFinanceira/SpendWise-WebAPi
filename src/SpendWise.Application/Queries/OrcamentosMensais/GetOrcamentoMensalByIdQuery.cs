using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.OrcamentosMensais;

public record GetOrcamentoMensalByIdQuery(
    Guid Id,
    Guid UsuarioId
) : IRequest<OrcamentoMensalDto?>;
