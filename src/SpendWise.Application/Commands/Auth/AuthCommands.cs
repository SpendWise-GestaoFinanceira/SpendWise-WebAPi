using MediatR;
using SpendWise.Application.DTOs.Auth;

namespace SpendWise.Application.Commands.Auth;

public record LoginCommand(
    string Email,
    string Senha
) : IRequest<LoginResponseDto>;

public record RegisterCommand(
    string Nome,
    string Email,
    string Senha,
    string ConfirmarSenha
) : IRequest<Guid>;
