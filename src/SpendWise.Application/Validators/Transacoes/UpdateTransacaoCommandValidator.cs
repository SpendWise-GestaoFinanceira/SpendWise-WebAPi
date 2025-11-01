using FluentValidation;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Validators.Transacoes;

public class UpdateTransacaoCommandValidator : AbstractValidator<UpdateTransacaoCommand>
{
    public UpdateTransacaoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id da transação é obrigatório");

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("Descrição é obrigatória")
            .MaximumLength(200)
            .WithMessage("Descrição deve ter no máximo 200 caracteres");

        RuleFor(x => x.Valor)
            .NotNull()
            .WithMessage("Valor é obrigatório")
            .Must(BePositiveValue)
            .WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x.DataTransacao)
            .NotEmpty()
            .WithMessage("Data da transação é obrigatória")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Data da transação não pode ser no futuro");

        RuleFor(x => x.CategoriaId)
            .NotEmpty()
            .WithMessage("Categoria é obrigatória");

        RuleFor(x => x.Observacoes)
            .MaximumLength(500)
            .WithMessage("Observações devem ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observacoes));
    }

    private static bool BePositiveValue(Money valor)
    {
        return valor?.Valor > 0;
    }
}
