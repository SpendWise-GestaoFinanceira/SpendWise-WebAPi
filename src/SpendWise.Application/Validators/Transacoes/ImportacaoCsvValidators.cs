using FluentValidation;
using SpendWise.Application.Commands.Transacoes;

namespace SpendWise.Application.Validators.Transacoes;

public class ProcessarArquivoCsvCommandValidator : AbstractValidator<ProcessarArquivoCsvCommand>
{
    public ProcessarArquivoCsvCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");

        RuleFor(x => x.ArquivoStream)
            .NotNull()
            .WithMessage("Stream do arquivo é obrigatório");

        RuleFor(x => x.NomeArquivo)
            .NotEmpty()
            .WithMessage("Nome do arquivo é obrigatório")
            .Must(fileName => fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Arquivo deve ter extensão .csv");
    }
}

public class ConfirmarImportacaoCsvCommandValidator : AbstractValidator<ConfirmarImportacaoCsvCommand>
{
    public ConfirmarImportacaoCsvCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("ID do usuário é obrigatório");

        RuleFor(x => x.Confirmacao.IdImportacao)
            .NotEmpty()
            .WithMessage("ID da importação é obrigatório");

        RuleFor(x => x.Confirmacao.MapeamentoCategorias)
            .NotNull()
            .WithMessage("Mapeamento de categorias não pode ser nulo");
    }
}
