using MediatR;
using Microsoft.Extensions.Logging;
using SpendWise.Application.DTOs.Transacoes;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Application.Services;
using SpendWise.Domain.Interfaces;
using AutoMapper;

namespace SpendWise.Application.Handlers.Transacoes;

public class ExportTransacoesHandler : IRequestHandler<ExportTransacoesQuery, ExportTransacoesResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExportService _exportService;
    private readonly IMapper _mapper;
    private readonly ILogger<ExportTransacoesHandler> _logger;

    public ExportTransacoesHandler(
        IUnitOfWork unitOfWork, 
        IExportService exportService, 
        IMapper mapper,
        ILogger<ExportTransacoesHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _exportService = exportService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ExportTransacoesResponseDto> Handle(ExportTransacoesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando export de transações para usuário {UsuarioId}", request.UsuarioId);

        try
        {
            // Buscar transações com filtros
            var transacoes = await BuscarTransacoesComFiltros(request.UsuarioId, request.Request);
            
            _logger.LogInformation("Encontradas {Count} transações para export", transacoes.Count());

            // Converter para DTO de export
            var transacoesExport = await ConverterParaExportDto(transacoes);

            // Gerar arquivo
            byte[] conteudoArquivo;
            string contentType;

            if (request.Request.Formato.ToUpper() == "JSON")
            {
                conteudoArquivo = await _exportService.ExportToJsonAsync(transacoesExport);
                contentType = "application/json";
            }
            else
            {
                conteudoArquivo = await _exportService.ExportToCsvAsync(transacoesExport, request.Request.IncluirCabecalho);
                contentType = "text/csv";
            }

            var nomeArquivo = _exportService.GerarNomeArquivo(
                request.Request.Formato, 
                request.Request.DataInicio, 
                request.Request.DataFim);

            var descricaoFiltros = _exportService.GerarDescricaoFiltros(request.Request);

            _logger.LogInformation("Export concluído: arquivo {NomeArquivo} com {Tamanho} bytes", 
                nomeArquivo, conteudoArquivo.Length);

            return new ExportTransacoesResponseDto
            {
                NomeArquivo = nomeArquivo,
                ContentType = contentType,
                ConteudoArquivo = conteudoArquivo,
                TotalRegistros = transacoesExport.Count(),
                DataGeracao = DateTime.UtcNow,
                Filtros = descricaoFiltros
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exportar transações para usuário {UsuarioId}", request.UsuarioId);
            throw;
        }
    }

    private async Task<IEnumerable<Domain.Entities.Transacao>> BuscarTransacoesComFiltros(Guid usuarioId, ExportTransacoesRequestDto request)
    {
        var todasTransacoes = await _unitOfWork.Transacoes.GetAllAsync();
        
        // Filtrar por usuário
        var query = todasTransacoes.Where(t => t.UsuarioId == usuarioId);

        // Aplicar filtros de data
        if (request.DataInicio.HasValue)
        {
            query = query.Where(t => t.DataTransacao >= request.DataInicio.Value);
        }

        if (request.DataFim.HasValue)
        {
            query = query.Where(t => t.DataTransacao <= request.DataFim.Value);
        }

        // Filtrar por categoria
        if (request.CategoriaId.HasValue)
        {
            query = query.Where(t => t.CategoriaId == request.CategoriaId.Value);
        }

        // Filtrar por tipo
        if (!string.IsNullOrEmpty(request.Tipo))
        {
            query = query.Where(t => t.Tipo.ToString() == request.Tipo);
        }

        // Aplicar ordenação
        switch (request.OrdenarPor?.ToLower())
        {
            case "valor":
                query = request.Crescente 
                    ? query.OrderBy(t => t.Valor.Valor)
                    : query.OrderByDescending(t => t.Valor.Valor);
                break;
            case "descricao":
                query = request.Crescente 
                    ? query.OrderBy(t => t.Descricao)
                    : query.OrderByDescending(t => t.Descricao);
                break;
            default: // "datatransacao"
                query = request.Crescente 
                    ? query.OrderBy(t => t.DataTransacao)
                    : query.OrderByDescending(t => t.DataTransacao);
                break;
        }

        return query.ToList();
    }

    private async Task<List<TransacaoExportDto>> ConverterParaExportDto(IEnumerable<Domain.Entities.Transacao> transacoes)
    {
        var resultado = new List<TransacaoExportDto>();

        foreach (var transacao in transacoes)
        {
            // Buscar nome da categoria
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(transacao.CategoriaId);
            var nomeCategoria = categoria?.Nome ?? "Categoria não encontrada";

            resultado.Add(new TransacaoExportDto
            {
                Data = transacao.DataTransacao,
                Descricao = transacao.Descricao,
                Valor = transacao.Valor.Valor,
                Tipo = transacao.Tipo.ToString(),
                Categoria = nomeCategoria,
                Observacoes = transacao.Observacoes,
                Moeda = transacao.Valor.Moeda
            });
        }

        return resultado;
    }
}
