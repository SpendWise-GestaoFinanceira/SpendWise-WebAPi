using MediatR;
using SpendWise.Application.DTOs.Categoria;

namespace SpendWise.Application.Commands.Categoria;

public class CreateCategoriaCommand : IRequest<Guid>
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Cor { get; set; }
    public Guid UsuarioId { get; set; }
}

public class UpdateCategoriaCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Cor { get; set; }
    public Guid UsuarioId { get; set; }
}

public class DeleteCategoriaCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
}
