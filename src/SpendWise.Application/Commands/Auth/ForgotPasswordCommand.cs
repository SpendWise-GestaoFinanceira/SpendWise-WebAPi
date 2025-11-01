using MediatR;
using SpendWise.Application.DTOs.Auth;

namespace SpendWise.Application.Commands.Auth;

public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResponseDto>;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword
) : IRequest<ResetPasswordResponseDto>;
