using FluentValidation;
using SpendWise.Application.Queries.Transacoes;

namespace SpendWise.Application.Validators.Transacoes;

public class ExportTransacoesQueryValidator : AbstractValidator<ExportTransacoesQuery>
{
    public ExportTransacoesQueryValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");

        RuleFor(x => x.Request.DataInicio)
            .LessThanOrEqualTo(x => x.Request.DataFim)
            .When(x => x.Request.DataInicio.HasValue && x.Request.DataFim.HasValue)
            .WithMessage("Data de início deve ser anterior ou igual à data de fim");

        RuleFor(x => x.Request.Formato)
            .NotEmpty()
            .WithMessage("Formato é obrigatório")
            .Must(formato => new[] { "CSV", "JSON" }.Contains(formato.ToUpper()))
            .WithMessage("Formato deve ser 'CSV' ou 'JSON'");

        RuleFor(x => x.Request.Tipo)
            .Must(tipo => string.IsNullOrEmpty(tipo) || 
                         tipo.Equals("Receita", StringComparison.OrdinalIgnoreCase) || 
                         tipo.Equals("Despesa", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Tipo deve ser 'Receita' ou 'Despesa'");

        RuleFor(x => x.Request.OrdenarPor)
            .Must(campo => string.IsNullOrEmpty(campo) || 
                          new[] { "DataTransacao", "Valor", "Descricao" }.Contains(campo, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Campo de ordenação deve ser: DataTransacao, Valor ou Descricao");

        // Validação de período máximo (opcional, para performance)
        RuleFor(x => x.Request)
            .Must(request => 
            {
                if (request.DataInicio.HasValue && request.DataFim.HasValue)
                {
                    var diasPeriodo = (request.DataFim.Value - request.DataInicio.Value).TotalDays;
                    return diasPeriodo <= 366; // Máximo 1 ano
                }
                return true;
            })
            .WithMessage("Período máximo para export é de 1 ano");
    }
}
