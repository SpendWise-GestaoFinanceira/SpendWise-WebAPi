using MediatR;
using SpendWise.Application.Commands.Auth;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.Auth;

public class RegisterHandler : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Validar se as senhas coincidem
        if (request.Senha != request.ConfirmarSenha)
        {
            throw new ArgumentException("As senhas não coincidem");
        }

        // Validar se o email já existe
        var email = new Email(request.Email);
        var usuarioExistente = await _usuarioRepository.BuscarPorEmailAsync(email);

        if (usuarioExistente != null)
        {
            throw new InvalidOperationException("Já existe um usuário com este email");
        }

        // Hash da senha
        var senhaHash = _passwordHasher.HashPassword(request.Senha);

        // Criar novo usuário
        var novoUsuario = new SpendWise.Domain.Entities.Usuario(
            request.Nome,
            email,
            senhaHash
        );

        // Salvar no banco
        await _usuarioRepository.AdicionarAsync(novoUsuario);
        await _usuarioRepository.SalvarAsync();

        return novoUsuario.Id;
    }
}
