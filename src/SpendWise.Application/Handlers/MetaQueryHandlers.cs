using AutoMapper;
using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Metas;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;

namespace SpendWise.Application.Handlers;

public class GetMetaByIdHandler : IRequestHandler<GetMetaByIdQuery, MetaDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMetaByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MetaDto?> Handle(GetMetaByIdQuery request, CancellationToken cancellationToken)
    {
        var meta = await _unitOfWork.Metas.GetByIdAsync(request.Id);
        return meta != null ? _mapper.Map<MetaDto>(meta) : null;
    }
}

public class GetMetasByUsuarioHandler : IRequestHandler<GetMetasByUsuarioQuery, IEnumerable<MetaResumoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMetasByUsuarioHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MetaResumoDto>> Handle(GetMetasByUsuarioQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Meta> metas;

        if (request.ApenasAtivas)
        {
            metas = await _unitOfWork.Metas.GetAtivasByUsuarioIdAsync(request.UsuarioId);
        }
        else
        {
            metas = await _unitOfWork.Metas.GetByUsuarioIdAsync(request.UsuarioId);
        }

        return _mapper.Map<IEnumerable<MetaResumoDto>>(metas);
    }
}

public class GetMetasVencidasHandler : IRequestHandler<GetMetasVencidasQuery, IEnumerable<MetaResumoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMetasVencidasHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MetaResumoDto>> Handle(GetMetasVencidasQuery request, CancellationToken cancellationToken)
    {
        var metas = await _unitOfWork.Metas.GetVencidasByUsuarioIdAsync(request.UsuarioId);
        return _mapper.Map<IEnumerable<MetaResumoDto>>(metas);
    }
}

public class GetMetasAlcancadasHandler : IRequestHandler<GetMetasAlcancadasQuery, IEnumerable<MetaResumoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMetasAlcancadasHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MetaResumoDto>> Handle(GetMetasAlcancadasQuery request, CancellationToken cancellationToken)
    {
        var metas = await _unitOfWork.Metas.GetAlcancadasByUsuarioIdAsync(request.UsuarioId);
        return _mapper.Map<IEnumerable<MetaResumoDto>>(metas);
    }
}

public class GetEstatisticasMetasHandler : IRequestHandler<GetEstatisticasMetasQuery, EstatisticasMetasDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEstatisticasMetasHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EstatisticasMetasDto> Handle(GetEstatisticasMetasQuery request, CancellationToken cancellationToken)
    {
        var todasMetas = await _unitOfWork.Metas.GetByUsuarioIdAsync(request.UsuarioId);
        var metasAtivas = await _unitOfWork.Metas.GetAtivasByUsuarioIdAsync(request.UsuarioId);
        var metasVencidas = await _unitOfWork.Metas.GetVencidasByUsuarioIdAsync(request.UsuarioId);
        var metasAlcancadas = await _unitOfWork.Metas.GetAlcancadasByUsuarioIdAsync(request.UsuarioId);

        var totalMetas = todasMetas.Count();
        var totalAtivas = metasAtivas.Count();
        var totalVencidas = metasVencidas.Count();
        var totalAlcancadas = metasAlcancadas.Count();

        var valorTotalObjetivos = metasAtivas.Sum(m => m.ValorObjetivo.Valor);
        var valorTotalProgresso = metasAtivas.Sum(m => m.ValorAtual.Valor);

        var percentualMedioProgresso = totalAtivas > 0 
            ? metasAtivas.Average(m => m.CalcularPercentualProgresso())
            : 0;

        var metasProximasVencimento = metasAtivas
            .Where(m => m.CalcularDiasRestantes() <= 30 && m.CalcularDiasRestantes() > 0)
            .Count();

        return new EstatisticasMetasDto
        {
            TotalMetas = totalMetas,
            MetasAtivas = totalAtivas,
            MetasVencidas = totalVencidas,
            MetasAlcancadas = totalAlcancadas,
            ValorTotalAlvo = valorTotalObjetivos,
            ValorTotalAtual = valorTotalProgresso,
            PercentualGeralProgresso = (decimal)percentualMedioProgresso
        };
    }
}

public class GetMetasResumoHandler : IRequestHandler<GetMetasResumoQuery, IEnumerable<MetaResumoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMetasResumoHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MetaResumoDto>> Handle(GetMetasResumoQuery request, CancellationToken cancellationToken)
    {
        var metas = await _unitOfWork.Metas.GetAtivasByUsuarioIdAsync(request.UsuarioId);
        
        // Ordenar por prioridade: mais próximas do vencimento primeiro
        var metasOrdenadas = metas
            .Where(m => m.IsAtiva)
            .OrderBy(m => m.CalcularDiasRestantes())
            .Take(request.Limite);

        return _mapper.Map<IEnumerable<MetaResumoDto>>(metasOrdenadas);
    }
}
