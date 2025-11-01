using FluentValidation;
using SpendWise.Application.Queries.Transacoes;

namespace SpendWise.Application.Validators.Transacoes;

public class GetTransacoesAdvancedQueryValidator : AbstractValidator<GetTransacoesAdvancedQuery>
{
    public GetTransacoesAdvancedQueryValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Página deve ser maior que 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Tamanho da página deve estar entre 1 e 100");

        RuleFor(x => x.DataInicio)
            .LessThanOrEqualTo(x => x.DataFim)
            .When(x => x.DataInicio.HasValue && x.DataFim.HasValue)
            .WithMessage("Data de início deve ser anterior ou igual à data de fim");

        RuleFor(x => x.ValorMinimo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ValorMinimo.HasValue)
            .WithMessage("Valor mínimo deve ser maior ou igual a 0");

        RuleFor(x => x.ValorMaximo)
            .GreaterThanOrEqualTo(x => x.ValorMinimo)
            .When(x => x.ValorMinimo.HasValue && x.ValorMaximo.HasValue)
            .WithMessage("Valor máximo deve ser maior ou igual ao valor mínimo");

        RuleFor(x => x.OrderBy)
            .Must(campo => string.IsNullOrEmpty(campo) || new[] { "DataTransacao", "Valor", "Descricao" }.Contains(campo))
            .WithMessage("Campo de ordenação deve ser: DataTransacao, Valor ou Descricao");
    }
}
