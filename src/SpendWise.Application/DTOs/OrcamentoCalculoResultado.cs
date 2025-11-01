namespace SpendWise.Application.DTOs;

public class OrcamentoCalculoResultado
{
    public decimal ValorOrcamento { get; set; }
    public decimal ValorGasto { get; set; }
    public decimal ValorRestante { get; set; }
    public decimal PercentualUtilizado { get; set; }
    public StatusOrcamento Status { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string MensagemStatus { get; set; } = string.Empty;
}

public enum StatusOrcamento
{
    Dentro = 0,     // 0-80%
    Atencao = 1,    // 80-95%
    Alerta = 2,     // 95-100%
    Excedido = 3    // > 100%
}
