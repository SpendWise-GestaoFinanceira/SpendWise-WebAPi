using SpendWise.Domain.Enums;

namespace SpendWise.Application.DTOs;

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Cor { get; set; }
    public TipoCategoria Tipo { get; set; }
    public Guid UsuarioId { get; set; }
    public bool IsAtiva { get; set; }
    public decimal? Limite { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CategoriaComProgressoDto : CategoriaDto
{
    public decimal GastoMensal { get; set; }
    public decimal PercentualUtilizado { get; set; }
    public StatusLimite StatusLimite { get; set; }
    public string MensagemStatus { get; set; } = string.Empty;
    public bool ProximoDoLimite => PercentualUtilizado >= 80;
    public decimal SaldoRestante => (Limite ?? 0) - GastoMensal;
}

public class CreateCategoriaDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoCategoria Tipo { get; set; }
    public Guid UsuarioId { get; set; }
    public decimal? Limite { get; set; }
}

public class UpdateCategoriaDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal? Limite { get; set; }
}
