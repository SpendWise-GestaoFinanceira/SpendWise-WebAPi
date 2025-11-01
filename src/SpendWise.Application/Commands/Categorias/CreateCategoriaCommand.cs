using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Enums;

namespace SpendWise.Application.Commands.Categorias;

public record CreateCategoriaCommand(
    string Nome,
    string? Descricao,
    string? Cor,
    TipoCategoria Tipo,
    Guid UsuarioId,
    decimal? Limite = null
) : IRequest<CategoriaDto>;