using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Categorias;

public record GetCategoriaByIdQuery(Guid Id) : IRequest<CategoriaDto?>;