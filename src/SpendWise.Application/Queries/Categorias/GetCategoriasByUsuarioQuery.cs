using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Categorias;

public record GetCategoriasByUsuarioQuery(Guid UsuarioId) : IRequest<IEnumerable<CategoriaDto>>;