using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Usuario;

public record GetUsuarioByIdQuery(Guid Id) : IRequest<UsuarioDto?>;
