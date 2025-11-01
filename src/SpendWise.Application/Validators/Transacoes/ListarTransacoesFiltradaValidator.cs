using FluentValidation;
using SpendWise.Application.Queries.Transacoes;

namespace SpendWise.Application.Validators.Transacoes;

public class ListarTransacoesFiltradaValidator : AbstractValidator<ListarTransacoesFiltradas>
{
    public ListarTransacoesFiltradaValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");

        RuleFor(x => x.Paginacao.Pagina)
            .GreaterThan(0)
            .WithMessage("Página deve ser maior que 0");

        RuleFor(x => x.Paginacao.TamanhoPagina)
            .InclusiveBetween(1, 100)
            .WithMessage("Tamanho da página deve estar entre 1 e 100");

        RuleFor(x => x.Filtros.DataInicio)
            .LessThanOrEqualTo(x => x.Filtros.DataFim)
            .When(x => x.Filtros.DataInicio.HasValue && x.Filtros.DataFim.HasValue)
            .WithMessage("Data de início deve ser anterior ou igual à data de fim");

        RuleFor(x => x.Filtros.ValorMinimo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Filtros.ValorMinimo.HasValue)
            .WithMessage("Valor mínimo deve ser maior ou igual a 0");

        RuleFor(x => x.Filtros.ValorMaximo)
            .GreaterThanOrEqualTo(x => x.Filtros.ValorMinimo)
            .When(x => x.Filtros.ValorMinimo.HasValue && x.Filtros.ValorMaximo.HasValue)
            .WithMessage("Valor máximo deve ser maior ou igual ao valor mínimo");

        RuleFor(x => x.Filtros.Tipo)
            .Must(tipo => string.IsNullOrEmpty(tipo) || tipo.Equals("Receita", StringComparison.OrdinalIgnoreCase) || tipo.Equals("Despesa", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Tipo deve ser 'Receita' ou 'Despesa'");

        RuleFor(x => x.Paginacao.OrdenarPor)
            .Must(campo => string.IsNullOrEmpty(campo) || new[] { "data", "valor", "descricao", "categoria" }.Contains(campo.ToLower()))
            .WithMessage("Campo de ordenação deve ser: data, valor, descricao ou categoria");

        RuleFor(x => x.Paginacao.Direcao)
            .Must(direcao => string.IsNullOrEmpty(direcao) || new[] { "asc", "desc" }.Contains(direcao.ToLower()))
            .WithMessage("Direção de ordenação deve ser 'asc' ou 'desc'");
    }
}
