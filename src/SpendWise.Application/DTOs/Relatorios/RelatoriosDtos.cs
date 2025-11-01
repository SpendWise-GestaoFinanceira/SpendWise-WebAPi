namespace SpendWise.Application.DTOs.Relatorios;

public class ResumoMensalDto
{
    public string AnoMes { get; set; } = string.Empty;
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Saldo { get; set; }
    public decimal OrcamentoPlanejado { get; set; }
    public decimal PercentualGasto { get; set; }
    public int QuantidadeTransacoes { get; set; }
    public string StatusMes { get; set; } = string.Empty; // "Superavit", "Deficit", "Equilibrado"
}

public class CategoriaSummaryDto
{
    public Guid CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public decimal TotalGasto { get; set; }
    public decimal LimiteCategoria { get; set; }
    public decimal PercentualLimite { get; set; }
    public int QuantidadeTransacoes { get; set; }
    public string StatusLimite { get; set; } = string.Empty;
}

public class EvolucaoGastosDto
{
    public List<MesGastoDto> UltimosDoozeMeses { get; set; } = new();
    public decimal MediaMensal { get; set; }
    public decimal MaiorGasto { get; set; }
    public decimal MenorGasto { get; set; }
    public string TendenciaGeral { get; set; } = string.Empty; // "Crescente", "Decrescente", "Estável"
}

public class MesGastoDto
{
    public string AnoMes { get; set; } = string.Empty;
    public decimal TotalGasto { get; set; }
    public decimal Orcamento { get; set; }
}

public class TopCategoriaDto
{
    public Guid CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public decimal TotalGasto { get; set; }
    public decimal PercentualDoTotal { get; set; }
    public int Posicao { get; set; }
}

public class OrcadoVsRealizadoDto
{
    public string AnoMes { get; set; } = string.Empty;
    public decimal OrcamentoTotal { get; set; }
    public decimal GastoRealizado { get; set; }
    public decimal Diferenca { get; set; }
    public decimal PercentualRealizado { get; set; }
    public List<CategoriaOrcadoVsRealizadoDto> CategoriaDetalhes { get; set; } = new();
}

public class CategoriaOrcadoVsRealizadoDto
{
    public Guid CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public decimal LimiteOrcado { get; set; }
    public decimal GastoRealizado { get; set; }
    public decimal Diferenca { get; set; }
    public decimal PercentualRealizado { get; set; }
}
