using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Interfaces;
using SpendWise.Application.Queries.Categorias;


namespace SpendWise.Application.Handlers.Categorias;

public class GetCategoriaByIdQueryHandler : IRequestHandler<GetCategoriaByIdQuery, CategoriaDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCategoriaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoriaDto?> Handle(GetCategoriaByIdQuery request, CancellationToken cancellationToken)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(request.Id);
        
        if (categoria == null)
            return null;

        return _mapper.Map<CategoriaDto>(categoria);
    }
}