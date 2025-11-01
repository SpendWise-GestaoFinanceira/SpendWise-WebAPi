using SpendWise.Domain.Enums;

namespace SpendWise.Application.DTOs;

public class TransacaoDto
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "BRL";
    public DateTime DataTransacao { get; set; }
    public TipoTransacao Tipo { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid CategoriaId { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Dados relacionados
    public string? CategoriaNome { get; set; }
    public string? CategoriaCor { get; set; }
    public string? UsuarioNome { get; set; }
    
    // Categoria completa para facilitar uso no frontend
    public CategoriaDto? Categoria { get; set; }
}

public class CreateTransacaoDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "BRL";
    public DateTime DataTransacao { get; set; }
    public TipoTransacao Tipo { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid CategoriaId { get; set; }
    public string? Observacoes { get; set; }
}

public class UpdateTransacaoDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Moeda { get; set; } = "BRL";
    public DateTime DataTransacao { get; set; }
    public Guid CategoriaId { get; set; }
    public string? Observacoes { get; set; }
}
