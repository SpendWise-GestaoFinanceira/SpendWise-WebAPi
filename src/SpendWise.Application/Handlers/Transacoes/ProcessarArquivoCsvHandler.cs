using MediatR;
using Microsoft.Extensions.Logging;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Application.DTOs.Transacoes;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Interfaces;
using System.Globalization;
using System.Text;
using AutoMapper;

namespace SpendWise.Application.Handlers.Transacoes;

public class ProcessarArquivoCsvHandler : IRequestHandler<ProcessarArquivoCsvCommand, PreVisualizacaoImportacaoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessarArquivoCsvHandler> _logger;
    private readonly IMapper _mapper;

    public ProcessarArquivoCsvHandler(IUnitOfWork unitOfWork, ILogger<ProcessarArquivoCsvHandler> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PreVisualizacaoImportacaoDto> Handle(ProcessarArquivoCsvCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando arquivo CSV: {NomeArquivo}", request.NomeArquivo);

        var importacao = new ImportacaoCsvDto
        {
            NomeArquivo = request.NomeArquivo,
            DataUpload = DateTime.UtcNow
        };

        var linhas = new List<LinhaImportacaoDto>();

        try
        {
            using var reader = new StreamReader(request.ArquivoStream, Encoding.UTF8);

            var numeroLinha = 0;
            string? linha;

            // Pular cabeçalho se existir
            if (!reader.EndOfStream)
            {
                linha = await reader.ReadLineAsync();
                numeroLinha++;
                
                // Verificar se é cabeçalho (contém texto comum em cabeçalhos)
                if (linha?.ToLower().Contains("data") == true || 
                    linha?.ToLower().Contains("descricao") == true ||
                    linha?.ToLower().Contains("valor") == true)
                {
                    _logger.LogInformation("Cabeçalho detectado e ignorado: {Cabecalho}", linha);
                }
                else
                {
                    // Não é cabeçalho, processar como primeira linha de dados
                    var linhaDto = await ProcessarLinha(linha ?? string.Empty, numeroLinha, request.UsuarioId);
                    linhas.Add(linhaDto);
                }
            }

            // Processar linhas de dados
            while (!reader.EndOfStream)
            {
                linha = await reader.ReadLineAsync();
                numeroLinha++;

                if (string.IsNullOrWhiteSpace(linha))
                    continue;

                var linhaDto = await ProcessarLinha(linha, numeroLinha, request.UsuarioId);
                linhas.Add(linhaDto);
            }

            importacao.Linhas = linhas;
            importacao.TotalLinhas = linhas.Count;
            importacao.LinhasValidas = linhas.Count(l => l.EhValida);
            importacao.LinhasComErro = linhas.Count(l => !l.EhValida);
            importacao.StatusProcessamento = "Processado";

            _logger.LogInformation("Arquivo processado: {TotalLinhas} linhas, {Validas} válidas, {Erros} com erro", 
                importacao.TotalLinhas, importacao.LinhasValidas, importacao.LinhasComErro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar arquivo CSV");
            importacao.StatusProcessamento = "Erro";
            
            // Adicionar linha de erro geral
            linhas.Add(new LinhaImportacaoDto
            {
                NumeroLinha = 0,
                EhValida = false,
                Erros = new List<string> { $"Erro ao processar arquivo: {ex.Message}" }
            });
        }

        // Buscar categorias do usuário
        var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(request.UsuarioId);
        var categoriasDto = _mapper.Map<List<CategoriaDto>>(categorias);

        return new PreVisualizacaoImportacaoDto
        {
            Importacao = importacao,
            CategoriasDisponiveis = categoriasDto,
            SugestoesMapeamento = GerarSugestoesMapeamento(linhas, categoriasDto)
        };
    }

    private async Task<LinhaImportacaoDto> ProcessarLinha(string linha, int numeroLinha, Guid usuarioId)
    {
        var linhaDto = new LinhaImportacaoDto
        {
            NumeroLinha = numeroLinha,
            EhValida = true
        };

        try
        {
            // Split por vírgula ou ponto e vírgula (CSV comum)
            var campos = linha.Split(new[] { ',', ';' }, StringSplitOptions.None)
                              .Select(c => c.Trim().Trim('"'))
                              .ToArray();

            if (campos.Length < 4)
            {
                linhaDto.EhValida = false;
                linhaDto.Erros.Add("Linha deve ter pelo menos 4 campos: Data, Descrição, Valor, Tipo");
                return linhaDto;
            }

            // Mapear campos (formato esperado: Data, Descrição, Valor, Tipo, [Categoria], [Observações])
            linhaDto.Data = campos[0];
            linhaDto.Descricao = campos[1];
            linhaDto.Valor = campos[2];
            linhaDto.Tipo = campos[3];
            
            if (campos.Length > 4)
                linhaDto.Categoria = campos[4];
            
            if (campos.Length > 5)
                linhaDto.Observacoes = campos[5];

            // Validar e parsear Data
            if (DateTime.TryParseExact(linhaDto.Data, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" }, 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataParsed))
            {
                linhaDto.DataParsed = dataParsed;
            }
            else
            {
                linhaDto.EhValida = false;
                linhaDto.Erros.Add($"Data inválida: {linhaDto.Data}. Formatos aceitos: dd/MM/yyyy, dd-MM-yyyy, yyyy-MM-dd");
            }

            // Validar e parsear Valor
            var valorLimpo = linhaDto.Valor.Replace("R$", "").Replace(" ", "").Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(valorLimpo, NumberStyles.Number, CultureInfo.InvariantCulture, out var valorParsed))
            {
                linhaDto.ValorParsed = Math.Abs(valorParsed); // Sempre positivo
            }
            else
            {
                linhaDto.EhValida = false;
                linhaDto.Erros.Add($"Valor inválido: {linhaDto.Valor}");
            }

            // Validar Tipo
            if (!string.Equals(linhaDto.Tipo, "Receita", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(linhaDto.Tipo, "Despesa", StringComparison.OrdinalIgnoreCase))
            {
                linhaDto.EhValida = false;
                linhaDto.Erros.Add($"Tipo inválido: {linhaDto.Tipo}. Deve ser 'Receita' ou 'Despesa'");
            }

            // Validar Descrição
            if (string.IsNullOrWhiteSpace(linhaDto.Descricao))
            {
                linhaDto.EhValida = false;
                linhaDto.Erros.Add("Descrição é obrigatória");
            }

            // Buscar categoria se especificada
            if (!string.IsNullOrWhiteSpace(linhaDto.Categoria))
            {
                var categorias = await _unitOfWork.Categorias.GetByUsuarioIdAsync(usuarioId);
                var categoria = categorias.FirstOrDefault(c => 
                    string.Equals(c.Nome, linhaDto.Categoria, StringComparison.OrdinalIgnoreCase));
                
                if (categoria != null)
                {
                    linhaDto.CategoriaId = categoria.Id;
                }
                else
                {
                    linhaDto.Erros.Add($"Categoria '{linhaDto.Categoria}' não encontrada");
                    // Não marcar como inválida - pode ser mapeada depois
                }
            }
        }
        catch (Exception ex)
        {
            linhaDto.EhValida = false;
            linhaDto.Erros.Add($"Erro ao processar linha: {ex.Message}");
        }

        return linhaDto;
    }

    private List<string> GerarSugestoesMapeamento(List<LinhaImportacaoDto> linhas, List<CategoriaDto> categorias)
    {
        var sugestoes = new List<string>();
        
        // Encontrar categorias mencionadas mas não encontradas
        var categoriasNaoEncontradas = linhas
            .Where(l => !string.IsNullOrWhiteSpace(l.Categoria) && !l.CategoriaId.HasValue)
            .Select(l => l.Categoria!)
            .Distinct()
            .ToList();

        foreach (var categoriaNaoEncontrada in categoriasNaoEncontradas)
        {
            // Buscar categoria similar
            var categoriaSimilar = categorias
                .FirstOrDefault(c => c.Nome.Contains(categoriaNaoEncontrada!, StringComparison.OrdinalIgnoreCase) ||
                                    categoriaNaoEncontrada!.Contains(c.Nome, StringComparison.OrdinalIgnoreCase));

            if (categoriaSimilar != null)
            {
                sugestoes.Add($"Mapear '{categoriaNaoEncontrada}' → '{categoriaSimilar.Nome}'");
            }
            else
            {
                sugestoes.Add($"Criar nova categoria: '{categoriaNaoEncontrada}'");
            }
        }

        return sugestoes;
    }
}
