using MediatR;
using Microsoft.Extensions.Logging;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Application.DTOs.Transacoes;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Entities;
using SpendWise.Domain.Interfaces;
using SpendWise.Domain.ValueObjects;
using SpendWise.Domain.Enums;
using AutoMapper;

namespace SpendWise.Application.Handlers.Transacoes;

public class ConfirmarImportacaoCsvHandler : IRequestHandler<ConfirmarImportacaoCsvCommand, ResultadoImportacaoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmarImportacaoCsvHandler> _logger;
    private readonly IMapper _mapper;
    private static readonly Dictionary<string, ImportacaoCsvDto> _importacoesEmCache = new();

    public ConfirmarImportacaoCsvHandler(IUnitOfWork unitOfWork, ILogger<ConfirmarImportacaoCsvHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ResultadoImportacaoDto> Handle(ConfirmarImportacaoCsvCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Confirmando importação CSV para usuário {UsuarioId}", request.UsuarioId);

        var resultado = new ResultadoImportacaoDto();

        try
        {
            // Em um cenário real, buscaríamos a importação do banco
            // Para este mock, simulamos com cache em memória
            if (!_importacoesEmCache.TryGetValue(request.Confirmacao.IdImportacao, out var importacao))
            {
                resultado.Sucesso = false;
                resultado.Erros.Add("Importação não encontrada ou expirada");
                return resultado;
            }

            var linhasParaImportar = importacao.Linhas.Where(l => l.EhValida);
            
            // Se específicas linhas foram selecionadas
            if (request.Confirmacao.LinhasParaImportar.Any())
            {
                linhasParaImportar = linhasParaImportar.Where(l => 
                    request.Confirmacao.LinhasParaImportar.Contains(l.NumeroLinha));
            }

            var transacoesCriadas = new List<TransacaoDto>();

            // Iniciar transação no banco
            await _unitOfWork.BeginTransactionAsync();

            foreach (var linha in linhasParaImportar)
            {
                try
                {
                    // Resolver categoria
                    var categoriaId = await ResolverCategoriaId(linha, request);
                    
                    if (!categoriaId.HasValue)
                    {
                        resultado.TransacoesComErro++;
                        resultado.Erros.Add($"Linha {linha.NumeroLinha}: Categoria não pôde ser resolvida");
                        continue;
                    }

                    // Verificar se o mês da transação está fechado
                    var anoMes = linha.DataParsed!.Value.ToString("yyyy-MM");
                    var mesEstaFechado = await _unitOfWork.FechamentosMensais.MesEstaFechadoAsync(request.UsuarioId, anoMes);
                    
                    if (mesEstaFechado)
                    {
                        resultado.TransacoesComErro++;
                        resultado.Erros.Add($"Linha {linha.NumeroLinha}: Mês {anoMes} está fechado, não é possível adicionar transações");
                        continue;
                    }

                    // Criar transação
                    var tipo = string.Equals(linha.Tipo, "Receita", StringComparison.OrdinalIgnoreCase) 
                        ? Domain.Enums.TipoTransacao.Receita 
                        : Domain.Enums.TipoTransacao.Despesa;

                    var transacao = new Domain.Entities.Transacao(
                        descricao: linha.Descricao,
                        valor: new Domain.ValueObjects.Money(linha.ValorParsed!.Value),
                        dataTransacao: linha.DataParsed!.Value,
                        tipo: tipo,
                        usuarioId: request.UsuarioId,
                        categoriaId: categoriaId.Value,
                        observacoes: linha.Observacoes
                    );

                    // Salvar no banco
                    await _unitOfWork.Transacoes.AddAsync(transacao);
                    
                    _logger.LogInformation("Transação criada: {Descricao} - {Valor} - {Data}", 
                        transacao.Descricao, transacao.Valor.Valor, transacao.DataTransacao);
                    
                    var transacaoDto = _mapper.Map<TransacaoDto>(transacao);
                    transacoesCriadas.Add(transacaoDto);
                    resultado.TransacoesCriadas++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao importar linha {NumeroLinha}", linha.NumeroLinha);
                    resultado.TransacoesComErro++;
                    resultado.Erros.Add($"Linha {linha.NumeroLinha}: {ex.Message}");
                }
            }

            // Salvar todas as mudanças
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            resultado.Sucesso = true;
            resultado.TransacoesImportadas = transacoesCriadas;

            // Limpar cache após importação
            _importacoesEmCache.Remove(request.Confirmacao.IdImportacao);

            _logger.LogInformation("Importação concluída: {Criadas} transações criadas, {Erros} erros", 
                resultado.TransacoesCriadas, resultado.TransacoesComErro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar importação CSV");
            await _unitOfWork.RollbackTransactionAsync();
            resultado.Sucesso = false;
            resultado.Erros.Add($"Erro geral na importação: {ex.Message}");
        }

        return resultado;
    }

    private async Task<Guid?> ResolverCategoriaId(LinhaImportacaoDto linha, ConfirmarImportacaoCsvCommand request)
    {
        // Se já tem categoria ID definida, usar ela
        if (linha.CategoriaId.HasValue)
            return linha.CategoriaId.Value;

        // Se não tem categoria especificada, retornar null
        if (string.IsNullOrWhiteSpace(linha.Categoria))
            return null;

        // Tentar mapear pela configuração do usuário
        if (request.Confirmacao.MapeamentoCategorias.TryGetValue(linha.Categoria, out var categoriaMapeada))
        {
            return categoriaMapeada;
        }

        // Buscar categoria pelo nome (case insensitive)
        var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);
        var categoria = categorias.FirstOrDefault(c => 
            string.Equals(c.Nome, linha.Categoria, StringComparison.OrdinalIgnoreCase));

        if (categoria != null)
            return categoria.Id;

        // Se chegou aqui, categoria não foi encontrada
        return null;
    }

    // Método auxiliar para armazenar importação em cache (simulação)
    public static void ArmazenarImportacaoEmCache(string id, ImportacaoCsvDto importacao)
    {
        _importacoesEmCache[id] = importacao;
        
        // Limpar cache antigo (simulação de TTL)
        var expirados = _importacoesEmCache
            .Where(kvp => kvp.Value.DataUpload < DateTime.UtcNow.AddHours(-1))
            .Select(kvp => kvp.Key)
            .ToList();
            
        foreach (var chave in expirados)
        {
            _importacoesEmCache.Remove(chave);
        }
    }
}
