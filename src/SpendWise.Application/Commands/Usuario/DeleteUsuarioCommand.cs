using MediatR;

namespace SpendWise.Application.Commands.Usuario;

public record DeleteUsuarioCommand(Guid Id) : IRequest<bool>;
