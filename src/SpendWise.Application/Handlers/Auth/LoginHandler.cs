using MediatR;
using SpendWise.Application.Commands.Auth;
using SpendWise.Application.DTOs.Auth;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.Auth;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuário por email
        var email = new Email(request.Email);
        var usuario = await _usuarioRepository.BuscarPorEmailAsync(email);

        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        // Verificar senha
        if (!_passwordHasher.VerifyPassword(request.Senha, usuario.Senha))
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        // Gerar token
        var token = _tokenService.GenerateToken(usuario.Id.ToString(), usuario.Email.Valor);

        return new LoginResponseDto
        {
            Token = token,
            User = new DTOs.Auth.UserDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email.Valor
            },
            ExpiresAt = DateTime.UtcNow.AddHours(1) // Configurar conforme o token
        };
    }
}
