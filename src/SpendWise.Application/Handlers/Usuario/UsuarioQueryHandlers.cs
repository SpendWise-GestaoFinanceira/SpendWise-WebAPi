using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Usuario;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers.Usuario;

public class GetUsuarioByIdQueryHandler : IRequestHandler<GetUsuarioByIdQuery, UsuarioDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsuarioByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioDto?> Handle(GetUsuarioByIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.BuscarPorIdAsync(request.Id);
        return usuario == null ? null : _mapper.Map<UsuarioDto>(usuario);
    }
}

public class GetAllUsuariosQueryHandler : IRequestHandler<GetAllUsuariosQuery, IEnumerable<UsuarioDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllUsuariosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UsuarioDto>> Handle(GetAllUsuariosQuery request, CancellationToken cancellationToken)
    {
        var usuarios = await _unitOfWork.Usuarios.BuscarTodosAsync();
        return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
    }
}
