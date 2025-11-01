using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Categorias;

public record GetCategoriasComProgressoQuery(Guid UsuarioId, DateTime? Data = null) : IRequest<IEnumerable<CategoriaComProgressoDto>>;
