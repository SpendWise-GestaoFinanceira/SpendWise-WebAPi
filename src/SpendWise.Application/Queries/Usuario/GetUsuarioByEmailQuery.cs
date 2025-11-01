using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Queries.Usuario;

public record GetUsuarioByEmailQuery(string Email) : IRequest<UsuarioDto?>;
