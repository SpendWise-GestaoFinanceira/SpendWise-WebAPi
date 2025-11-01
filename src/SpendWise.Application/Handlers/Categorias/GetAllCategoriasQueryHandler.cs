using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Categorias;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Categorias;

public class GetAllCategoriasQueryHandler : IRequestHandler<GetAllCategoriasQuery, IEnumerable<CategoriaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCategoriasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoriaDto>> Handle(GetAllCategoriasQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _unitOfWork.Categorias.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }
}