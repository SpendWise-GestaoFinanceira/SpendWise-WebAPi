using SpendWise.Domain.Enums;

namespace SpendWise.Application.DTOs;

public class FechamentoMensalDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string AnoMes { get; set; } = string.Empty;
    public DateTime DataFechamento { get; set; }
    public StatusFechamento Status { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal SaldoFinal { get; set; }
    public string? Observacoes { get; set; }
}
