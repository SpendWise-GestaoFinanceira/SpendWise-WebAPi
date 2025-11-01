namespace SpendWise.Application.DTOs.Transacoes;

public class ExportTransacoesRequestDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public Guid? CategoriaId { get; set; }
    public string? Tipo { get; set; } // "Receita" ou "Despesa"
    public string Formato { get; set; } = "CSV"; // "CSV" ou "JSON"
    public bool IncluirCabecalho { get; set; } = true;
    public string? OrdenarPor { get; set; } = "DataTransacao"; // DataTransacao, Valor, Descricao
    public bool Crescente { get; set; } = false; // false = mais recentes primeiro
}

public class ExportTransacoesResponseDto
{
    public string NomeArquivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] ConteudoArquivo { get; set; } = Array.Empty<byte>();
    public int TotalRegistros { get; set; }
    public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
    public string Filtros { get; set; } = string.Empty; // Descrição dos filtros aplicados
}

public class TransacaoExportDto
{
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public string Moeda { get; set; } = "BRL";
}
