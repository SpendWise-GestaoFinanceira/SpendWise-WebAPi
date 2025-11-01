using MediatR;
using SpendWise.Application.DTOs.Categorias;

namespace SpendWise.Application.Commands.Categorias;

public record ReatribuirDespesasCommand(
    Guid UsuarioId,
    Guid CategoriaOrigemId,
    Guid CategoriaDestinoId
) : IRequest<ReatribuirDespesasResponseDto>;

public record DeleteCategoriaWithReatribuicaoCommand(
    Guid UsuarioId,
    Guid CategoriaId,
    Guid? CategoriaDestinoId = null
) : IRequest<bool>;
