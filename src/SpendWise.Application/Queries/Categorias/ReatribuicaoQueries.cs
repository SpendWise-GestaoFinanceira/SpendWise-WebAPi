using MediatR;
using SpendWise.Application.DTOs.Categorias;

namespace SpendWise.Application.Queries.Categorias;

public record GetPreviewExclusaoCategoriaQuery(
    Guid UsuarioId,
    Guid CategoriaId
) : IRequest<PreviewExclusaoCategoriaDto>;
