using FluentValidation;
using SpendWise.Application.Commands.Transacoes;

namespace SpendWise.Application.Validators.Transacoes;

public class DeleteTransacaoCommandValidator : AbstractValidator<DeleteTransacaoCommand>
{
    public DeleteTransacaoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id da transação é obrigatório");
    }
}
