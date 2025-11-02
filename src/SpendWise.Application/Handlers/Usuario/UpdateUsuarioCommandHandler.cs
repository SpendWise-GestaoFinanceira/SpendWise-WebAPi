using AutoMapper;
using MediatR;
using SpendWise.Application.Commands.Usuario;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Usuario;

public class UpdateUsuarioCommandHandler : IRequestHandler<UpdateUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUsuarioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioDto> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.Id);

        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado");

        usuario.AtualizarNome(request.Nome);
        usuario.AtualizarRendaMensal(request.RendaMensal);

        await _unitOfWork.Usuarios.AtualizarAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UsuarioDto>(usuario);
    }
}
