using FluentValidation;
using SpendWise.Application.DTOs.Relatorios;

namespace SpendWise.Application.Validators.Relatorios;

public class ComparativoMesesRequestValidator : AbstractValidator<ComparativoMesesRequestDto>
{
    public ComparativoMesesRequestValidator()
    {
        RuleFor(x => x.AnoInicio)
            .GreaterThan(2000)
            .LessThanOrEqualTo(DateTime.Now.Year + 1)
            .WithMessage("Ano de início deve estar entre 2001 e o próximo ano");

        RuleFor(x => x.MesInicio)
            .GreaterThan(0)
            .LessThanOrEqualTo(12)
            .WithMessage("Mês de início deve estar entre 1 e 12");

        RuleFor(x => x.AnoFim)
            .GreaterThan(2000)
            .LessThanOrEqualTo(DateTime.Now.Year + 1)
            .WithMessage("Ano de fim deve estar entre 2001 e o próximo ano");

        RuleFor(x => x.MesFim)
            .GreaterThan(0)
            .LessThanOrEqualTo(12)
            .WithMessage("Mês de fim deve estar entre 1 e 12");

        RuleFor(x => x)
            .Must(ValidarPeriodo)
            .WithMessage("A data de início deve ser anterior ou igual à data de fim");

        RuleFor(x => x)
            .Must(ValidarLimitePeriodo)
            .WithMessage("O período não pode exceder 36 meses");

        RuleFor(x => x.CategoriaIds)
            .Must(ValidarCategorias)
            .WithMessage("Lista de categorias não pode estar vazia quando fornecida")
            .When(x => x.CategoriaIds is not null);
    }

    private bool ValidarPeriodo(ComparativoMesesRequestDto request)
    {
        var dataInicio = new DateTime(request.AnoInicio, request.MesInicio, 1);
        var dataFim = new DateTime(request.AnoFim, request.MesFim, 1);

        return dataInicio <= dataFim;
    }

    private bool ValidarLimitePeriodo(ComparativoMesesRequestDto request)
    {
        var dataInicio = new DateTime(request.AnoInicio, request.MesInicio, 1);
        var dataFim = new DateTime(request.AnoFim, request.MesFim, 1);

        var diferencaMeses = ((dataFim.Year - dataInicio.Year) * 12) + dataFim.Month - dataInicio.Month + 1;

        return diferencaMeses <= 36;
    }

    private bool ValidarCategorias(List<Guid>? categoriaIds)
    {
        return categoriaIds?.Any() ?? true;
    }
}
