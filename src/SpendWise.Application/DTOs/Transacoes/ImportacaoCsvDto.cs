namespace SpendWise.Application.DTOs.Transacoes;

public class ImportacaoCsvDto
{
    public string NomeArquivo { get; set; } = string.Empty;
    public List<LinhaImportacaoDto> Linhas { get; set; } = new();
    public DateTime DataUpload { get; set; } = DateTime.UtcNow;
    public string StatusProcessamento { get; set; } = "Pendente";
    public int TotalLinhas { get; set; }
    public int LinhasValidas { get; set; }
    public int LinhasComErro { get; set; }
}

public class LinhaImportacaoDto
{
    public int NumeroLinha { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public bool EhValida { get; set; }
    public List<string> Erros { get; set; } = new();
    
    // Dados parseados (se válidos)
    public DateTime? DataParsed { get; set; }
    public decimal? ValorParsed { get; set; }
    public Guid? CategoriaId { get; set; }
}

public class PreVisualizacaoImportacaoDto
{
    public ImportacaoCsvDto Importacao { get; set; } = new();
    public List<CategoriaDto> CategoriasDisponiveis { get; set; } = new();
    public List<string> SugestoesMapeamento { get; set; } = new();
}

public class ConfirmacaoImportacaoDto
{
    public string IdImportacao { get; set; } = string.Empty;
    public List<int> LinhasParaImportar { get; set; } = new(); // Se vazio, importa todas as válidas
    public Dictionary<string, Guid> MapeamentoCategorias { get; set; } = new(); // Nome categoria -> ID
}

public class ResultadoImportacaoDto
{
    public bool Sucesso { get; set; }
    public int TransacoesCriadas { get; set; }
    public int TransacoesComErro { get; set; }
    public List<string> Erros { get; set; } = new();
    public List<TransacaoDto> TransacoesImportadas { get; set; } = new();
}
