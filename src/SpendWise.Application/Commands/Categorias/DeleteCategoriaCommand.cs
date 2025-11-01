using MediatR;

namespace SpendWise.Application.Commands.Categorias;

public record DeleteCategoriaCommand(Guid Id) : IRequest<bool>;