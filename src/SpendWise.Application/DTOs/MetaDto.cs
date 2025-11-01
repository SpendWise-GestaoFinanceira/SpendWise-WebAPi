namespace SpendWise.Application.DTOs;

public class MetaDto
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorAlvo { get; set; }
    public string MoedaAlvo { get; set; } = "BRL";
    public decimal ValorAtual { get; set; }
    public string MoedaAtual { get; set; } = "BRL";
    public DateTime Prazo { get; set; }
    public Guid UsuarioId { get; set; }
    public bool IsAtiva { get; set; }
    public DateTime? DataAlcancada { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Propriedades calculadas
    public decimal PercentualProgresso { get; set; }
    public decimal ValorRestante { get; set; }
    public int DiasRestantes { get; set; }
    public bool IsAlcancada { get; set; }
    public bool IsVencida { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public DateTime? ProjecaoAlcance { get; set; }
}

public class MetaResumoDto
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorAlvo { get; set; }
    public decimal ValorAtual { get; set; }
    public decimal PercentualProgresso { get; set; }
    public DateTime Prazo { get; set; }
    public int DiasRestantes { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public bool IsAlcancada { get; set; }
    public bool IsVencida { get; set; }
}

public class EstatisticasMetasDto
{
    public int TotalMetas { get; set; }
    public int MetasAtivas { get; set; }
    public int MetasAlcancadas { get; set; }
    public int MetasVencidas { get; set; }
    public decimal ValorTotalAlvo { get; set; }
    public decimal ValorTotalAtual { get; set; }
    public decimal PercentualGeralProgresso { get; set; }
    public MetaResumoDto? MetaMaisProxima { get; set; }
    public MetaResumoDto? MetaMaiorValor { get; set; }
}
