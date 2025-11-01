using SpendWise.Application.Commands.Usuario;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.Usuario;

public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUsuarioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioDto> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
    {
        // Verificar se email já existe
        if (await _unitOfWork.Usuarios.EmailExisteAsync(new Email(request.Email)))
        {
            throw new InvalidOperationException("Email já está em uso");
        }

        // Hash da senha (implementação simples - em produção usar BCrypt)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Criar entidade
        var usuario = new Domain.Entities.Usuario(
            request.Nome,
            new Email(request.Email),
            passwordHash,
            request.RendaMensal
        );

        // Salvar no banco
        await _unitOfWork.Usuarios.AdicionarAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        // Retornar DTO
        return _mapper.Map<UsuarioDto>(usuario);
    }
}
