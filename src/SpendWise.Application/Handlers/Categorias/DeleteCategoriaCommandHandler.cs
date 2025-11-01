using MediatR;
using SpendWise.Application.Commands.Categorias;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Categorias;

public class DeleteCategoriaCommandHandler : IRequestHandler<DeleteCategoriaCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoriaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(request.Id);
        
        if (categoria == null)
            return false;

        // Usar DeleteAsync em vez de Delete
        await _unitOfWork.Categorias.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}