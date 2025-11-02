using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using SpendWise.Application.Commands.Auth;
using SpendWise.Application.DTOs.Auth;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Auth;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<ForgotPasswordCommand> _validator;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPasswordHasher passwordHasher,
        IValidator<ForgotPasswordCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<ForgotPasswordResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var usuario = await _unitOfWork.Usuarios.BuscarPorEmailAsync(request.Email);

        // Por segurança, sempre retornar sucesso mesmo se usuário não existir
        if (usuario == null)
        {
            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "Se o email estiver cadastrado, você receberá instruções para redefinir sua senha."
            };
        }

        // Gerar token seguro
        var token = GenerateSecureToken();
        var validPeriod = TimeSpan.FromMinutes(30); // Token válido por 30 minutos

        usuario.DefinirTokenResetSenha(token, validPeriod);
        await _unitOfWork.SaveChangesAsync();

        // Construir URL de reset (hardcoded para desenvolvimento, em produção viria de configuração)
        var baseUrl = "http://localhost:3000"; // Frontend base URL
        var resetUrl = $"{baseUrl}/redefinir-senha?token={token}&email={request.Email}";

        // Enviar email
        var emailSent = await _emailService.SendPasswordResetEmailAsync(
            usuario.Email.Valor,
            token,
            resetUrl);

        return new ForgotPasswordResponseDto
        {
            Success = emailSent,
            Message = emailSent
                ? "Se o email estiver cadastrado, você receberá instruções para redefinir sua senha."
                : "Erro temporário. Tente novamente em alguns minutos."
        };
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<ResetPasswordCommand> _validator;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IValidator<ResetPasswordCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<ResetPasswordResponseDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var usuario = await _unitOfWork.Usuarios.BuscarPorEmailAsync(request.Email);
        if (usuario == null)
        {
            return new ResetPasswordResponseDto
            {
                Success = false,
                Message = "Token inválido ou expirado."
            };
        }

        if (!usuario.IsTokenResetSenhaValido(request.Token))
        {
            return new ResetPasswordResponseDto
            {
                Success = false,
                Message = "Token inválido ou expirado."
            };
        }

        // Hash da nova senha
        var senhaHash = _passwordHasher.HashPassword(request.NewPassword);

        // Resetar senha
        usuario.ResetarSenha(senhaHash);
        await _unitOfWork.SaveChangesAsync();

        return new ResetPasswordResponseDto
        {
            Success = true,
            Message = "Senha redefinida com sucesso. Você já pode fazer login com sua nova senha."
        };
    }
}
