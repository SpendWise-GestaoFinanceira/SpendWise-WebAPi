using AutoMapper;
using MediatR;
using SpendWise.Application.Commands.Categorias;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.Categorias;

public class CreateCategoriaCommandHandler : IRequestHandler<CreateCategoriaCommand, CategoriaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCategoriaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoriaDto> Handle(CreateCategoriaCommand request, CancellationToken cancellationToken)
    {
        // Validar se o usuário existe
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }

        Money? limite = request.Limite.HasValue ? new Money(request.Limite.Value) : null;
        var categoria = new Categoria(request.Nome, request.Tipo, request.UsuarioId, request.Descricao, limite);

        // Definir cor se fornecida
        if (!string.IsNullOrEmpty(request.Cor))
        {
            categoria.AtualizarCor(request.Cor);
        }

        var result = await _unitOfWork.Categorias.AddAsync(categoria);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoriaDto>(result);
    }
}
