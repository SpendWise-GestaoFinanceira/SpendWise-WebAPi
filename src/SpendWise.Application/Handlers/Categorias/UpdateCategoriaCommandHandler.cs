using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Commands.Categorias;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.Categorias;

public class UpdateCategoriaCommandHandler : IRequestHandler<UpdateCategoriaCommand, CategoriaDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCategoriaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoriaDto?> Handle(UpdateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(request.Id);
        
        if (categoria == null)
            return null;

        // Usar métodos da entidade em vez de setters
        categoria.AtualizarNome(request.Nome);
        categoria.AtualizarDescricao(request.Descricao);
        categoria.AtualizarCor(request.Cor);
        
        // Atualizar limite se fornecido
        if (request.Limite.HasValue)
        {
            var limite = new Money(request.Limite.Value);
            categoria.AtualizarLimite(limite);
        }
        else
        {
            categoria.AtualizarLimite(null);
        }
        
        if (request.IsAtiva)
            categoria.Ativar();
        else
            categoria.Desativar();

        // Usar UpdateAsync em vez de Update
        await _unitOfWork.Categorias.UpdateAsync(categoria);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoriaDto>(categoria);
    }
}