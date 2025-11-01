using MediatR;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.OrcamentosMensais;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;

namespace SpendWise.Application.Handlers.OrcamentosMensais;

public class GetEstatisticasCategoriasQueryHandler : IRequestHandler<GetEstatisticasCategoriasQuery, EstatisticasCategoriasDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEstatisticasCategoriasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EstatisticasCategoriasDto> Handle(GetEstatisticasCategoriasQuery request, CancellationToken cancellationToken)
    {
        // Parsear anoMes (formato YYYY-MM) para criar período
        var parts = request.AnoMes.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var ano) || !int.TryParse(parts[1], out var mes))
        {
            throw new ArgumentException("Formato de anoMes inválido. Use YYYY-MM");
        }

        var inicioMes = new DateTime(ano, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes), 23, 59, 59, DateTimeKind.Utc);
        var periodo = new Periodo(inicioMes, fimMes);

        var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);
        var transacoes = await _unitOfWork.Transacoes.GetByPeriodoAsync(request.UsuarioId, periodo);

        var categoriasComOrcamento = new List<OrcamentoCategoriaDto>();

        foreach (var categoria in categorias)
        {
            var gastoCategoria = transacoes
                .Where(t => t.CategoriaId == categoria.Id && t.Tipo == Domain.Enums.TipoTransacao.Despesa)
                .Sum(t => t.Valor.Valor);

            var limiteCategoria = categoria.Limite?.Valor ?? 0;
            var percentual = limiteCategoria > 0
                ? (gastoCategoria / limiteCategoria) * 100
                : 0;

            StatusOrcamento status;
            if (percentual >= 100)
                status = StatusOrcamento.Excedido;
            else if (percentual >= 95)
                status = StatusOrcamento.Alerta;
            else if (percentual >= 80)
                status = StatusOrcamento.Atencao;
            else
                status = StatusOrcamento.Dentro;

            categoriasComOrcamento.Add(new OrcamentoCategoriaDto
            {
                CategoriaId = categoria.Id,
                Nome = categoria.Nome,
                Limite = limiteCategoria,
                Gasto = gastoCategoria,
                PercentualUtilizado = percentual,
                Status = status
            });
        }

        return new EstatisticasCategoriasDto
        {
            AnoMes = request.AnoMes,
            Categorias = categoriasComOrcamento
        };
    }
}
