using FluentValidation;
using SpendWise.Application.Commands.Categorias;

namespace SpendWise.Application.Validators.Categorias;

public class ReatribuirDespesasCommandValidator : AbstractValidator<ReatribuirDespesasCommand>
{
    public ReatribuirDespesasCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");

        RuleFor(x => x.CategoriaOrigemId)
            .NotEmpty()
            .WithMessage("Categoria de origem é obrigatória");

        RuleFor(x => x.CategoriaDestinoId)
            .NotEmpty()
            .WithMessage("Categoria de destino é obrigatória");

        RuleFor(x => x)
            .Must(x => x.CategoriaOrigemId != x.CategoriaDestinoId)
            .WithMessage("Categoria de origem deve ser diferente da categoria de destino");
    }
}

public class DeleteCategoriaWithReatribuicaoCommandValidator : AbstractValidator<DeleteCategoriaWithReatribuicaoCommand>
{
    public DeleteCategoriaWithReatribuicaoCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");

        RuleFor(x => x.CategoriaId)
            .NotEmpty()
            .WithMessage("ID da categoria é obrigatório");

        RuleFor(x => x)
            .Must(x => x.CategoriaDestinoId == null || x.CategoriaId != x.CategoriaDestinoId)
            .WithMessage("Categoria de destino deve ser diferente da categoria a ser excluída");
    }
}
