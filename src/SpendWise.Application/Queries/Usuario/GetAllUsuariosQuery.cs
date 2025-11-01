using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Usuario;

public record GetAllUsuariosQuery() : IRequest<IEnumerable<UsuarioDto>>;
