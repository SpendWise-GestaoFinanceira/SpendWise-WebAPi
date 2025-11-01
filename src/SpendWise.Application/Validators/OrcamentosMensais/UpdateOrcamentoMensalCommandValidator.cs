using FluentValidation;
using SpendWise.Application.Commands.OrcamentosMensais;

namespace SpendWise.Application.Validators.OrcamentosMensais;

public class UpdateOrcamentoMensalCommandValidator : AbstractValidator<UpdateOrcamentoMensalCommand>
{
    public UpdateOrcamentoMensalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("UsuarioId é obrigatório");

        RuleFor(x => x.Valor.Valor)
            .GreaterThan(0).WithMessage("Valor do orçamento deve ser maior que zero")
            .LessThanOrEqualTo(1000000).WithMessage("Valor do orçamento não pode exceder R$ 1.000.000,00");

        RuleFor(x => x.Valor.Moeda)
            .NotEmpty().WithMessage("Moeda é obrigatória")
            .Length(3).WithMessage("Moeda deve ter 3 caracteres")
            .Must(BeValidCurrency).WithMessage("Moeda deve ser uma das suportadas: BRL, USD, EUR");
    }

    private static bool BeValidCurrency(string moeda)
    {
        var moedasValidas = new[] { "BRL", "USD", "EUR" };
        return moedasValidas.Contains(moeda?.ToUpper());
    }
}
