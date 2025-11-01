namespace SpendWise.Application.DTOs.Relatorios;

public class ComparativoMesesRequestDto
{
    public int AnoInicio { get; set; }
    public int MesInicio { get; set; } = 1;
    public int AnoFim { get; set; }
    public int MesFim { get; set; } = 12;
    public bool IncluirDetalhes { get; set; } = false;
    public List<Guid>? CategoriaIds { get; set; }
}

public class ComparativoMesesResponseDto
{
    public List<ResumoMensalComparativoDto> ResumosMensais { get; set; } = new();
    public EstatisticasComparativasDto Estatisticas { get; set; } = new();
    public List<TendenciaDto> Tendencias { get; set; } = new();
    public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
}

public class ResumoMensalComparativoDto
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string AnoMes { get; set; } = string.Empty; // "2024-01"
    public string NomeMes { get; set; } = string.Empty; // "Janeiro 2024"
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal SaldoLiquido { get; set; }
    public decimal PercentualEconomia { get; set; }
    public int QuantidadeTransacoes { get; set; }
    public bool MesFechado { get; set; }
    public List<ResumoCategoriaDto>? DetalhesCategorias { get; set; }
}

public class ResumoCategoriaDto
{
    public Guid CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public decimal ValorReceitas { get; set; }
    public decimal ValorDespesas { get; set; }
    public decimal SaldoCategoria { get; set; }
    public int QuantidadeTransacoes { get; set; }
}

public class EstatisticasComparativasDto
{
    public decimal MediaReceitas { get; set; }
    public decimal MediaDespesas { get; set; }
    public decimal MediaSaldo { get; set; }
    public ResumoMensalComparativoDto MelhorMes { get; set; } = new();
    public ResumoMensalComparativoDto PiorMes { get; set; } = new();
    public decimal MaiorReceita { get; set; }
    public decimal MaiorDespesa { get; set; }
    public decimal VariacaoPercentualReceitas { get; set; }
    public decimal VariacaoPercentualDespesas { get; set; }
}

public class TendenciaDto
{
    public string Indicador { get; set; } = string.Empty; // "Receitas", "Despesas", "Saldo"
    public string Tendencia { get; set; } = string.Empty; // "Crescente", "Decrescente", "Estável"
    public decimal VariacaoPercentual { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
