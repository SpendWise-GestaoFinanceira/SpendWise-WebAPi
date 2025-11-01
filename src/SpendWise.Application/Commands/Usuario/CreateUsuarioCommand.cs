using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Commands.Usuario;

public record CreateUsuarioCommand(
    string Nome,
    string Email,
    string Password,
    decimal RendaMensal
) : IRequest<UsuarioDto>;
