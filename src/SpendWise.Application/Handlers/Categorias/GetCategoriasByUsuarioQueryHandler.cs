using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Interfaces;
using SpendWise.Application.Queries.Categorias;


namespace SpendWise.Application.Handlers.Categorias;

public class GetCategoriasByUsuarioQueryHandler : IRequestHandler<GetCategoriasByUsuarioQuery, IEnumerable<CategoriaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCategoriasByUsuarioQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoriaDto>> Handle(GetCategoriasByUsuarioQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);
        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }
}
