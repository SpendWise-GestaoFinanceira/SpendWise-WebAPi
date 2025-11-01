using FluentValidation;
using SpendWise.Application.Commands.Categorias;

namespace SpendWise.Application.Validators.Categorias;

public class DeleteCategoriaCommandValidator : AbstractValidator<DeleteCategoriaCommand>
{
    public DeleteCategoriaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");
    }
}
