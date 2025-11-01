using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Categorias;

public record GetAllCategoriasQuery() : IRequest<IEnumerable<CategoriaDto>>;
