using FluentValidation;
using SpendWise.Application.Commands.Categorias;

namespace SpendWise.Application.Validators.Categorias;

public class UpdateCategoriaCommandValidator : AbstractValidator<UpdateCategoriaCommand>
{
    public UpdateCategoriaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter pelo menos 2 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de categoria inválido");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descricao));
    }
}
