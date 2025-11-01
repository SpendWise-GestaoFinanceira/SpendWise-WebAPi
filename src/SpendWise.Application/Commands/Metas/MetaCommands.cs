using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Commands.Metas;

public record CreateMetaCommand(
    string Nome,
    string Descricao,
    decimal ValorObjetivo,
    DateTime Prazo,
    Guid UsuarioId,
    decimal ValorAtual = 0
) : IRequest<MetaDto>;

public record UpdateMetaCommand(
    Guid Id,
    string? Nome = null,
    string? Descricao = null,
    decimal? ValorObjetivo = null,
    DateTime? Prazo = null
) : IRequest<MetaDto>;

public record UpdateProgressoMetaCommand(
    Guid MetaId,
    decimal ValorProgresso
) : IRequest<MetaDto>;

public record DeleteMetaCommand(
    Guid Id
) : IRequest<bool>;

public record ToggleMetaStatusCommand(
    Guid MetaId
) : IRequest<MetaDto>;
