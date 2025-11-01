using FluentValidation;
using SpendWise.Application.Commands.Usuario;

namespace SpendWise.Application.Validators.Usuario;

public class DeleteUsuarioCommandValidator : AbstractValidator<DeleteUsuarioCommand>
{
    public DeleteUsuarioCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id é obrigatório");
    }
}
