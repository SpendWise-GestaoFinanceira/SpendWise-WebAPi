using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Enums;

namespace SpendWise.Application.Commands.Categorias;

public record UpdateCategoriaCommand(
    Guid Id,
    string Nome,
    string? Descricao,
    string? Cor,
    TipoCategoria Tipo,
    bool IsAtiva,
    decimal? Limite = null
) : IRequest<CategoriaDto?>;