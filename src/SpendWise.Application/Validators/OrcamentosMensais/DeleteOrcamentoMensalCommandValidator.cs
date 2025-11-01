using FluentValidation;
using SpendWise.Application.Commands.OrcamentosMensais;

namespace SpendWise.Application.Validators.OrcamentosMensais;

public class DeleteOrcamentoMensalCommandValidator : AbstractValidator<DeleteOrcamentoMensalCommand>
{
    public DeleteOrcamentoMensalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("UsuarioId é obrigatório");
    }
}
