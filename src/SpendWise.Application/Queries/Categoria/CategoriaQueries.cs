using MediatR;
using SpendWise.Application.DTOs.Categoria;

namespace SpendWise.Application.Queries.Categoria;

public class GetCategoriaByIdQuery : IRequest<CategoriaDto?>
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
}

public class GetCategoriasByUsuarioQuery : IRequest<IEnumerable<CategoriaDto>>
{
    public Guid UsuarioId { get; set; }
}

public class GetCategoriasAtivasQuery : IRequest<IEnumerable<CategoriaDto>>
{
    public Guid UsuarioId { get; set; }
}
