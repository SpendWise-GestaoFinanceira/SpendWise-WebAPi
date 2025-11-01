using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Metas;

public record GetMetaByIdQuery(
    Guid Id
) : IRequest<MetaDto?>;

public record GetMetasByUsuarioQuery(
    Guid UsuarioId,
    bool ApenasAtivas = false
) : IRequest<IEnumerable<MetaResumoDto>>;

public record GetMetasVencidasQuery(
    Guid UsuarioId
) : IRequest<IEnumerable<MetaResumoDto>>;

public record GetMetasAlcancadasQuery(
    Guid UsuarioId
) : IRequest<IEnumerable<MetaResumoDto>>;

public record GetEstatisticasMetasQuery(
    Guid UsuarioId
) : IRequest<EstatisticasMetasDto>;

public record GetMetasResumoQuery(
    Guid UsuarioId,
    int Limite = 5
) : IRequest<IEnumerable<MetaResumoDto>>;
