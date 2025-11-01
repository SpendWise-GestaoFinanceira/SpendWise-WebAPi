using FluentValidation;
using SpendWise.Application.Commands.Auth;

namespace SpendWise.Application.Validators.Auth;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório")
            .EmailAddress()
            .WithMessage("Email deve ter um formato válido");
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório")
            .EmailAddress()
            .WithMessage("Email deve ter um formato válido");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token é obrigatório");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Nova senha é obrigatória")
            .MinimumLength(6)
            .WithMessage("Senha deve ter pelo menos 6 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Senha deve conter pelo menos uma letra minúscula, uma maiúscula e um número");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirmação de senha é obrigatória")
            .Equal(x => x.NewPassword)
            .WithMessage("Confirmação de senha deve ser igual à nova senha");
    }
}
