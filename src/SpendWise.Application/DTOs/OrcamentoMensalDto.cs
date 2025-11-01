using SpendWise.Domain.ValueObjects;
using SpendWise.Application.Services;

using SpendWise.Application.DTOs;

namespace SpendWise.Application.DTOs;

public class OrcamentoMensalDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string AnoMes { get; set; } = string.Empty;
    public Money Valor { get; set; } = new Money(0, "BRL");
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Propriedades calculadas básicas
    public decimal ValorGasto { get; set; }
    public decimal ValorRestante { get; set; }
    public decimal PercentualUtilizado { get; set; }
    
    // Propriedades calculadas avançadas
    public StatusOrcamento Status { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string MensagemStatus { get; set; } = string.Empty;
}

public class CreateOrcamentoMensalDto
{
    public Guid UsuarioId { get; set; }
    public string AnoMes { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "BRL";
}

public class UpdateOrcamentoMensalDto
{
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "BRL";
}
