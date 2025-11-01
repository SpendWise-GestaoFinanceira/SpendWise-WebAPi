using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Categorias;
using SpendWise.Domain.Enums;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.Categorias;

public class GetCategoriasComProgressoQueryHandler : IRequestHandler<GetCategoriasComProgressoQuery, IEnumerable<CategoriaComProgressoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCategoriasComProgressoQueryHandler> _logger;

    public GetCategoriasComProgressoQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCategoriasComProgressoQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoriaComProgressoDto>> Handle(GetCategoriasComProgressoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando categorias com progresso para usuário {UsuarioId}", request.UsuarioId);

        var data = request.Data ?? DateTime.Now;
        var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);

        var categoriasComProgresso = new List<CategoriaComProgressoDto>();

        foreach (var categoria in categorias)
        {
            var categoriaDto = _mapper.Map<CategoriaComProgressoDto>(categoria);

            // Calcular gasto mensal
            var gastoMensal = await CalcularGastoMensalCategoria(categoria.Id, data, cancellationToken);
            categoriaDto.GastoMensal = gastoMensal;

            // Calcular percentual e status
            if (categoria.Limite?.Valor > 0)
            {
                categoriaDto.PercentualUtilizado = categoria.CalcularPercentualUtilizado(gastoMensal);
                categoriaDto.StatusLimite = categoria.VerificarStatusLimite(gastoMensal);
                categoriaDto.MensagemStatus = ObterMensagemStatus(categoriaDto.StatusLimite, categoriaDto.PercentualUtilizado);
            }
            else
            {
                categoriaDto.PercentualUtilizado = 0;
                categoriaDto.StatusLimite = StatusLimite.SemLimite;
                categoriaDto.MensagemStatus = "Sem limite definido";
            }

            categoriasComProgresso.Add(categoriaDto);
        }

        return categoriasComProgresso.OrderBy(c => c.Nome);
    }

    private async Task<decimal> CalcularGastoMensalCategoria(Guid categoriaId, DateTime data, CancellationToken cancellationToken)
    {
        var inicioMes = new DateTime(data.Year, data.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var transacoes = await _unitOfWork.Transacoes.BuscarPorPeriodoComCategoriasAsync(
            Guid.Empty, // Será filtrado pela categoria
            inicioMes,
            fimMes,
            new List<Guid> { categoriaId },
            cancellationToken);

        return transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor.Valor);
    }

    private string ObterMensagemStatus(StatusLimite status, decimal percentual)
    {
        return status switch
        {
            StatusLimite.SemLimite => "Sem limite definido",
            StatusLimite.Normal => $"Dentro do limite ({percentual:F1}% usado)",
            StatusLimite.Alerta => $"⚠️ Próximo do limite ({percentual:F1}% usado)",
            StatusLimite.Excedido => $"🚨 Limite ultrapassado ({percentual:F1}% usado)",
            _ => "Status indefinido"
        };
    }
}
