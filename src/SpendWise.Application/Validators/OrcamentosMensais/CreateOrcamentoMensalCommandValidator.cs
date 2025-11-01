using FluentValidation;
using SpendWise.Application.Commands.OrcamentosMensais;
using System.Text.RegularExpressions;

namespace SpendWise.Application.Validators.OrcamentosMensais;

public class CreateOrcamentoMensalCommandValidator : AbstractValidator<CreateOrcamentoMensalCommand>
{
    public CreateOrcamentoMensalCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("UsuarioId é obrigatório");

        RuleFor(x => x.AnoMes)
            .NotEmpty().WithMessage("AnoMes é obrigatório")
            .Must(BeValidAnoMes).WithMessage("AnoMes deve estar no formato YYYY-MM")
            .Must(NotBeFutureDate).WithMessage("Não é possível criar orçamento para meses futuros além do próximo mês");

        RuleFor(x => x.Valor.Valor)
            .GreaterThan(0).WithMessage("Valor do orçamento deve ser maior que zero")
            .LessThanOrEqualTo(1000000).WithMessage("Valor do orçamento não pode exceder R$ 1.000.000,00");

        RuleFor(x => x.Valor.Moeda)
            .NotEmpty().WithMessage("Moeda é obrigatória")
            .Length(3).WithMessage("Moeda deve ter 3 caracteres")
            .Must(BeValidCurrency).WithMessage("Moeda deve ser uma das suportadas: BRL, USD, EUR");
    }

    private static bool BeValidAnoMes(string anoMes)
    {
        if (string.IsNullOrEmpty(anoMes)) return false;
        
        var regex = new Regex(@"^\d{4}-\d{2}$");
        if (!regex.IsMatch(anoMes)) return false;

        var parts = anoMes.Split('-');
        if (parts.Length != 2) return false;

        return int.TryParse(parts[0], out var ano) && 
               int.TryParse(parts[1], out var mes) &&
               ano >= 2020 && ano <= 2030 &&
               mes >= 1 && mes <= 12;
    }

    private static bool NotBeFutureDate(string anoMes)
    {
        if (!BeValidAnoMes(anoMes)) return false;

        var parts = anoMes.Split('-');
        var ano = int.Parse(parts[0]);
        var mes = int.Parse(parts[1]);
        
        var dataOrcamento = new DateTime(ano, mes, 1);
        var proximoMes = DateTime.Today.AddMonths(1);
        var inicioProximoMes = new DateTime(proximoMes.Year, proximoMes.Month, 1);

        return dataOrcamento <= inicioProximoMes;
    }

    private static bool BeValidCurrency(string moeda)
    {
        var moedasValidas = new[] { "BRL", "USD", "EUR" };
        return moedasValidas.Contains(moeda?.ToUpper());
    }
}
