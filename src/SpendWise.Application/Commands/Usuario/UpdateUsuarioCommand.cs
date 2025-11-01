using MediatR;
using SpendWise.Application.DTOs;

namespace SpendWise.Application.Commands.Usuario;

public record UpdateUsuarioCommand(
    Guid Id,
    string Nome,
    decimal RendaMensal
) : IRequest<UsuarioDto>;
