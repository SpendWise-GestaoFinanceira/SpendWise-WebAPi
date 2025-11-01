using FluentValidation;
using SpendWise.Application.Commands.Transacoes;

namespace SpendWise.Application.Validators.Transacoes;

public class CreateTransacaoCommandValidator : AbstractValidator<CreateTransacaoCommand>
{
    public CreateTransacaoCommandValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MinimumLength(3).WithMessage("Descrição deve ter pelo menos 3 caracteres")
            .MaximumLength(200).WithMessage("Descrição deve ter no máximo 200 caracteres");

        RuleFor(x => x.Valor)
            .NotNull().WithMessage("Valor é obrigatório");

        RuleFor(x => x.Valor.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .When(x => x.Valor is not null);

        RuleFor(x => x.DataTransacao)
            .NotEmpty().WithMessage("Data da transação é obrigatória")
            .LessThanOrEqualTo(DateTime.Now.Date).WithMessage("Data não pode ser no futuro");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de transação inválido");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("UsuarioId é obrigatório");

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("CategoriaId é obrigatório para despesas")
            .When(x => x.Tipo == Domain.Enums.TipoTransacao.Despesa);

        RuleFor(x => x.Observacoes)
            .MaximumLength(1000).WithMessage("Observações devem ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observacoes));
    }
}